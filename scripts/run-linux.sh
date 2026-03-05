#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
APP_DIR="$ROOT_DIR/RiddleHub"

fail() {
  echo "ERROR: $*" >&2
  exit 1
}

has_cmd() {
  command -v "$1" >/dev/null 2>&1
}

run_with_sudo() {
  if [[ "$EUID" -eq 0 ]]; then
    "$@"
  else
    sudo "$@"
  fi
}

detect_xsp_cmd() {
  if has_cmd xsp4; then
    echo "xsp4"
  elif has_cmd xsp; then
    echo "xsp"
  fi
}

detect_aur_helper() {
  if has_cmd yay; then
    echo "yay"
  elif has_cmd paru; then
    echo "paru"
  fi
}

install_repo_packages() {
  local pkgs=("$@")
  [[ "${#pkgs[@]}" -gt 0 ]] || return 0
  has_cmd pacman || fail "Auto-install requires pacman (Arch Linux)."
  echo "==> Installing Arch packages: ${pkgs[*]}"
  run_with_sudo pacman -S --needed "${pkgs[@]}"
}

install_aur_xsp() {
  local helper
  helper="$(detect_aur_helper || true)"
  if [[ -n "$helper" ]]; then
    echo "==> Installing AUR package: xsp (via $helper)"
    "$helper" -S --needed xsp
    return 0
  fi

  fail "Cannot auto-install AUR package 'xsp' because no AUR helper (yay/paru) is installed."
}

DOCKER_USE_SUDO=0

docker_exec() {
  if [[ "${DOCKER_USE_SUDO:-0}" == "1" ]]; then
    run_with_sudo docker "$@"
  else
    docker "$@"
  fi
}

ensure_docker_daemon() {
  if docker info >/dev/null 2>&1; then
    DOCKER_USE_SUDO=0
    return 0
  fi

  if has_cmd systemctl; then
    echo "==> Starting docker daemon (systemctl enable --now docker)"
    run_with_sudo systemctl enable --now docker || true
  elif has_cmd service; then
    echo "==> Starting docker daemon (service docker start)"
    run_with_sudo service docker start || true
  fi

  if docker info >/dev/null 2>&1; then
    DOCKER_USE_SUDO=0
    return 0
  fi

  if run_with_sudo docker info >/dev/null 2>&1; then
    DOCKER_USE_SUDO=1
    echo "WARN: Docker requires sudo for this user; using sudo docker automatically."
    return 0
  fi

  fail "Docker daemon is not available. Tried to start it automatically but failed."
}

is_tcp_open() {
  local host="$1"
  local port="$2"
  timeout 1 bash -c ":</dev/tcp/$host/$port" >/dev/null 2>&1
}

wait_for_tcp() {
  local host="$1"
  local port="$2"
  local timeout_seconds="${3:-60}"
  local elapsed=0
  while [[ "$elapsed" -lt "$timeout_seconds" ]]; do
    if is_tcp_open "$host" "$port"; then
      return 0
    fi
    sleep 1
    elapsed=$((elapsed + 1))
  done
  return 1
}

ensure_local_sql_server() {
  if [[ "$RIDDLEHUB_CONNECTION_NAME" != "LinuxSqlServerDev" ]]; then
    return
  fi

  RIDDLEHUB_SQL_HOST="${RIDDLEHUB_SQL_HOST:-127.0.0.1}"
  RIDDLEHUB_SQL_PORT="${RIDDLEHUB_SQL_PORT:-1433}"
  RIDDLEHUB_SQL_CONTAINER="${RIDDLEHUB_SQL_CONTAINER:-riddlehub-sql}"
  RIDDLEHUB_SQL_IMAGE="${RIDDLEHUB_SQL_IMAGE:-mcr.microsoft.com/mssql/server:2022-latest}"
  RIDDLEHUB_SQL_PASSWORD="${RIDDLEHUB_SQL_PASSWORD:-Your_strong_Password123}"

  if is_tcp_open "$RIDDLEHUB_SQL_HOST" "$RIDDLEHUB_SQL_PORT"; then
    echo "Detected SQL Server at $RIDDLEHUB_SQL_HOST:$RIDDLEHUB_SQL_PORT"
    return
  fi

  if [[ "$RIDDLEHUB_SQL_HOST" != "127.0.0.1" && "$RIDDLEHUB_SQL_HOST" != "localhost" ]]; then
    echo "WARN: SQL Server is not reachable at $RIDDLEHUB_SQL_HOST:$RIDDLEHUB_SQL_PORT and host is remote; cannot auto-start remote DB."
    return
  fi

  if ! has_cmd docker; then
    if [[ "$RIDDLEHUB_AUTO_INSTALL" == "1" ]]; then
      install_repo_packages docker
    else
      echo "WARN: docker not found; SQL Server is still unreachable at $RIDDLEHUB_SQL_HOST:$RIDDLEHUB_SQL_PORT."
      return
    fi
  fi

  ensure_docker_daemon

  if docker_exec ps -a --format '{{.Names}}' | grep -Fxq "$RIDDLEHUB_SQL_CONTAINER"; then
    if ! docker_exec ps --format '{{.Names}}' | grep -Fxq "$RIDDLEHUB_SQL_CONTAINER"; then
      echo "==> Starting SQL Server container: $RIDDLEHUB_SQL_CONTAINER"
      docker_exec start "$RIDDLEHUB_SQL_CONTAINER" >/dev/null
    fi
  else
    echo "==> Creating SQL Server container: $RIDDLEHUB_SQL_CONTAINER"
    docker_exec run -d \
      --name "$RIDDLEHUB_SQL_CONTAINER" \
      -e ACCEPT_EULA=Y \
      -e MSSQL_SA_PASSWORD="$RIDDLEHUB_SQL_PASSWORD" \
      -p "$RIDDLEHUB_SQL_PORT:1433" \
      "$RIDDLEHUB_SQL_IMAGE" >/dev/null
  fi

  echo "==> Waiting for SQL Server on $RIDDLEHUB_SQL_HOST:$RIDDLEHUB_SQL_PORT"
  wait_for_tcp "$RIDDLEHUB_SQL_HOST" "$RIDDLEHUB_SQL_PORT" 90 || fail "SQL Server container started but port $RIDDLEHUB_SQL_PORT is still not reachable."
}

mono_module_loaded() {
  has_cmd apachectl && apachectl -M 2>/dev/null | grep -qi 'mono_module'
}

print_arch_host_help() {
  cat <<'HELP'
Install Linux host packages on Arch:
  # Preferred host stack
  sudo pacman -S --needed apache
  # mod_mono may be packaged in AUR/community depending on your setup.

  # Legacy fallback host stack from AUR (opt-in only; archived upstream)
  yay -S xsp
  # or: git clone https://aur.archlinux.org/xsp.git && cd xsp && makepkg -si
HELP
}

export RIDDLEHUB_CONNECTION_NAME="${RIDDLEHUB_CONNECTION_NAME:-LinuxSqlServerDev}"
PORT="${PORT:-8080}"
RIDDLEHUB_SERVER="${RIDDLEHUB_SERVER:-auto}"
APACHE_CONF="${RIDDLEHUB_APACHE_CONF:-}"
RIDDLEHUB_ALLOW_LEGACY_XSP="${RIDDLEHUB_ALLOW_LEGACY_XSP:-0}"
RIDDLEHUB_AUTO_INSTALL="${RIDDLEHUB_AUTO_INSTALL:-1}"
RIDDLEHUB_XSP_APPLICATIONS="${RIDDLEHUB_XSP_APPLICATIONS:-/:$APP_DIR}"
XSP_CMD="$(detect_xsp_cmd || true)"

if [[ "$RIDDLEHUB_SERVER" == "auto" ]]; then
  if mono_module_loaded; then
    RIDDLEHUB_SERVER="apache-mod_mono"
  else
    if [[ -z "$XSP_CMD" && "$RIDDLEHUB_AUTO_INSTALL" == "1" ]]; then
      install_aur_xsp
      XSP_CMD="$(detect_xsp_cmd || true)"
    fi

    if [[ -n "$XSP_CMD" ]]; then
      if [[ "$RIDDLEHUB_ALLOW_LEGACY_XSP" != "1" ]]; then
        echo "WARN: Apache mono_module unavailable; enabling legacy xsp fallback for this run."
        RIDDLEHUB_ALLOW_LEGACY_XSP=1
      fi
      RIDDLEHUB_SERVER="xsp4"
    else
      if [[ "$RIDDLEHUB_AUTO_INSTALL" == "1" ]] && ! has_cmd apachectl; then
        install_repo_packages apache
      fi
      print_arch_host_help
      if has_cmd apachectl; then
        fail "Apache is installed but mono_module is not loaded, and legacy xsp fallback is unavailable."
      fi
      fail "No supported host found. Configure Apache with mod_mono, or install AUR package 'xsp'."
    fi
  fi
fi

echo "Starting RiddleHub"
echo "  App directory: $APP_DIR"
echo "  Port: $PORT"
echo "  Connection profile: $RIDDLEHUB_CONNECTION_NAME"
echo "  Host stack: $RIDDLEHUB_SERVER"

ensure_local_sql_server
echo "Tip: SQL password must match Web.config LinuxSqlServerDev (or set env RIDDLEHUB_SQL_PASSWORD to override)."

case "$RIDDLEHUB_SERVER" in
  apache-mod_mono)
    if ! has_cmd apachectl; then
      if [[ "$RIDDLEHUB_AUTO_INSTALL" == "1" ]]; then
        install_repo_packages apache
      else
        print_arch_host_help
        fail "apachectl is required for apache-mod_mono mode."
      fi
    fi
    mono_module_loaded || fail "Apache mono_module is not loaded. Enable mod_mono in Apache first."
    if [[ -n "$APACHE_CONF" && ! -f "$APACHE_CONF" ]]; then
      fail "RIDDLEHUB_APACHE_CONF points to missing file: $APACHE_CONF"
    fi
    if [[ -z "$APACHE_CONF" ]]; then
      echo "WARN: RIDDLEHUB_APACHE_CONF is not set; ensure your Apache vhost is configured for: $APP_DIR"
      echo "      Example template: scripts/apache-riddlehub.conf.example"
    else
      echo "Using Apache config file: $APACHE_CONF"
    fi
    echo "  Open URL: http://localhost:$PORT/index.aspx"
    exec apachectl -DFOREGROUND
    ;;
  xsp4|xsp)
    if [[ "$RIDDLEHUB_ALLOW_LEGACY_XSP" != "1" ]]; then
      if [[ "$RIDDLEHUB_AUTO_INSTALL" == "1" ]]; then
        echo "WARN: Enabling legacy xsp fallback for this run."
        RIDDLEHUB_ALLOW_LEGACY_XSP=1
      else
        fail "xsp4 is disabled by default because upstream is archived. Re-run with RIDDLEHUB_ALLOW_LEGACY_XSP=1 if you need legacy fallback."
      fi
    fi
    if [[ -z "$XSP_CMD" ]]; then
      if [[ "$RIDDLEHUB_AUTO_INSTALL" == "1" ]]; then
        install_aur_xsp
        XSP_CMD="$(detect_xsp_cmd || true)"
      fi
      [[ -n "$XSP_CMD" ]] || {
        print_arch_host_help
        fail "xsp4/xsp is required for legacy mode. Install AUR package 'xsp'."
      }
    fi
    echo "WARN: $XSP_CMD is a legacy fallback (mono/xsp upstream is archived). Prefer apache-mod_mono when possible."
    echo "  Open URL: http://localhost:$PORT/index.aspx"
    exec "$XSP_CMD" --port "$PORT" --applications "$RIDDLEHUB_XSP_APPLICATIONS"
    ;;
  *)
    fail "Invalid RIDDLEHUB_SERVER='$RIDDLEHUB_SERVER'. Use: auto, apache-mod_mono, xsp4, or xsp."
    ;;
esac
