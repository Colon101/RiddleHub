#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

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
  has_cmd pacman || fail "Auto-install requires pacman (Arch Linux). Install packages manually on this OS."
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

mono_module_loaded() {
  has_cmd apachectl && apachectl -M 2>/dev/null | grep -qi 'mono_module'
}

apache_vhost_hint() {
  local conf="${RIDDLEHUB_APACHE_CONF:-}"
  if [[ -n "$conf" ]]; then
    [[ -f "$conf" ]] || fail "RIDDLEHUB_APACHE_CONF points to missing file: $conf"
  else
    echo "WARN: RIDDLEHUB_APACHE_CONF is not set; bootstrap cannot verify your RiddleHub Apache vhost file."
    echo "      See scripts/apache-riddlehub.conf.example for a template."
  fi
}

print_arch_install_help() {
  cat <<'HELP'
Install prerequisites on Arch Linux:

  # Required to restore/build this repo:
  sudo pacman -S --needed mono mono-msbuild nuget

  # Pick one Linux host option:
  # Preferred: Apache + mod_mono
  sudo pacman -S --needed apache
  # mod_mono may be packaged in AUR/community depending on your setup.

  # Legacy fallback host: xsp/xsp4 from AUR (opt-in only; archived upstream)
  # With an AUR helper:
  yay -S xsp
  # Or manually:
  # git clone https://aur.archlinux.org/xsp.git && cd xsp && makepkg -si

Optional (for local SQL Server container):
  sudo pacman -S --needed docker
HELP
}

print_support_note() {
  cat <<'NOTE'
Support note:
  ASP.NET Web Forms is part of .NET Framework, and .NET Framework is Windows-only.
  Linux hosting in this repo is best-effort via Mono (legacy path).
  For Microsoft-supported hosting, run this app on Windows + IIS.
NOTE
}

echo "==> Validating prerequisites"
print_support_note
echo
print_arch_install_help
echo
RIDDLEHUB_AUTO_INSTALL="${RIDDLEHUB_AUTO_INSTALL:-1}"
RIDDLEHUB_ALLOW_LEGACY_XSP="${RIDDLEHUB_ALLOW_LEGACY_XSP:-0}"

if [[ "$RIDDLEHUB_AUTO_INSTALL" == "1" ]]; then
  echo "Auto-install mode: enabled (set RIDDLEHUB_AUTO_INSTALL=0 to disable)"
else
  echo "Auto-install mode: disabled"
fi

MISSING_PKGS=()
if ! has_cmd mono; then
  MISSING_PKGS+=(mono)
fi
if ! has_cmd nuget; then
  MISSING_PKGS+=(nuget)
fi
if ! has_cmd msbuild && ! has_cmd xbuild; then
  MISSING_PKGS+=(mono-msbuild)
fi
if ! has_cmd apachectl; then
  MISSING_PKGS+=(apache)
fi

if [[ "${#MISSING_PKGS[@]}" -gt 0 ]]; then
  if [[ "$RIDDLEHUB_AUTO_INSTALL" == "1" ]]; then
    install_repo_packages "${MISSING_PKGS[@]}"
  else
    fail "Missing required packages: ${MISSING_PKGS[*]}"
  fi
fi

if has_cmd msbuild; then
  MSBUILD_CMD="msbuild"
elif has_cmd xbuild; then
  MSBUILD_CMD="xbuild"
else
  fail "Build tool still missing after install. Ensure mono-msbuild is installed."
fi

echo "Found build tool: $MSBUILD_CMD"
has_cmd mono || fail "mono is still missing after install."
has_cmd nuget || fail "nuget is still missing after install."

XSP_CMD="$(detect_xsp_cmd || true)"

if mono_module_loaded; then
  HOST_STACK="apache-mod_mono"
  apache_vhost_hint
else
  if [[ -z "$XSP_CMD" && "$RIDDLEHUB_AUTO_INSTALL" == "1" ]]; then
    echo "WARN: Apache mono_module is not loaded; attempting legacy xsp fallback install from AUR."
    install_aur_xsp
    XSP_CMD="$(detect_xsp_cmd || true)"
  fi

  if [[ -n "$XSP_CMD" ]]; then
    if [[ "$RIDDLEHUB_ALLOW_LEGACY_XSP" != "1" ]]; then
      echo "WARN: Enabling legacy xsp fallback for this bootstrap run (RIDDLEHUB_ALLOW_LEGACY_XSP=1)."
      RIDDLEHUB_ALLOW_LEGACY_XSP=1
    fi
    HOST_STACK="xsp-legacy"
    echo "WARN: $XSP_CMD fallback enabled (dev-only; mono/xsp upstream is archived)."
  elif has_cmd apachectl; then
    fail "Apache is installed but mono_module is not loaded, and legacy xsp fallback is unavailable."
  else
    fail "No runnable Linux host found. Configure Apache + mod_mono, or install AUR package 'xsp' for legacy fallback."
  fi
fi

echo "Detected Linux hosting stack: $HOST_STACK"

if has_cmd docker; then
  HAS_DOCKER=1
  echo "Found optional tool: docker"
else
  HAS_DOCKER=0
  echo "WARN: docker not found. You can still use an external SQL Server and keep LinuxSqlServerDev in Web.config pointed at it."
fi

echo "==> Restoring NuGet packages"
nuget restore RiddleHub.sln

echo "==> Building solution"
"$MSBUILD_CMD" RiddleHub.sln /p:Configuration=Debug

if [[ "$HOST_STACK" == "xsp-legacy" ]]; then
  RUN_APP_CMD="RIDDLEHUB_ALLOW_LEGACY_XSP=1 RIDDLEHUB_SERVER=xsp4 ./scripts/run-linux.sh"
else
  RUN_APP_CMD="./scripts/run-linux.sh"
fi

echo
if [[ "$HAS_DOCKER" -eq 1 ]]; then
  cat <<CMDS
Bootstrap succeeded.

Set RIDDLEHUB_SQL_PASSWORD and RIDDLEHUB_ADMIN_PASSWORD from your secret store.
For Apache, also set RIDDLEHUB_APACHE_CONF to the enabled loopback-only vhost.

Run app (Docker SQL is auto-managed):
  export RIDDLEHUB_CONNECTION_NAME=LinuxSqlServerDev
  $RUN_APP_CMD

Notes:
  - run-linux.sh auto-starts docker daemon when needed.
  - The SQL container and development HTTP listener bind to loopback only.
CMDS
else
  cat <<CMDS
Bootstrap succeeded.

Run commands (external SQL Server):
  1) Configure the LinuxSqlServerDev host/user with Encrypt=True in RiddleHub/Web.config.
  2) Set RIDDLEHUB_SQL_PASSWORD and RIDDLEHUB_ADMIN_PASSWORD from your secret store.
  3) export RIDDLEHUB_CONNECTION_NAME=LinuxSqlServerDev
  4) For Apache, set RIDDLEHUB_APACHE_CONF to the enabled loopback-only vhost.
  5) $RUN_APP_CMD
CMDS
fi
