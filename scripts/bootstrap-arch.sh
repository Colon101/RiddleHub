#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

fail() {
  echo "ERROR: $*" >&2
  exit 1
}

need_cmd() {
  local cmd="$1"
  local hint="$2"
  command -v "$cmd" >/dev/null 2>&1 || fail "Missing required tool '$cmd'. $hint"
}

has_cmd() {
  command -v "$1" >/dev/null 2>&1
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

echo "==> Validating prerequisites"
need_cmd mono "Install with: sudo pacman -S mono"

MSBUILD_CMD=""
if has_cmd msbuild; then
  MSBUILD_CMD="msbuild"
elif has_cmd xbuild; then
  MSBUILD_CMD="xbuild"
else
  fail "Missing build tool. Install mono-msbuild or mono-xbuild (pacman package: mono-msbuild)."
fi

echo "Found build tool: $MSBUILD_CMD"
need_cmd nuget "Install with: sudo pacman -S nuget"

HOST_STACK=""
if mono_module_loaded; then
  HOST_STACK="apache-mod_mono"
  apache_vhost_hint
elif has_cmd xsp4; then
  HOST_STACK="xsp4-legacy"
  echo "WARN: xsp4 detected. Upstream mono/xsp is archived; treat xsp4 as legacy/dev-only fallback."
else
  fail "No runnable Linux host found. Install Apache + mod_mono (preferred) or install legacy fallback xsp4 (mono-xsp)."
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

echo
if [[ "$HAS_DOCKER" -eq 1 ]]; then
  cat <<'CMDS'
Bootstrap succeeded.

Run commands (SQL Server in Docker):
  docker run -d --name riddlehub-sql \
    -e ACCEPT_EULA=Y \
    -e MSSQL_SA_PASSWORD='Your_strong_Password123' \
    -p 1433:1433 \
    mcr.microsoft.com/mssql/server:2022-latest

Then run app:
  export RIDDLEHUB_CONNECTION_NAME=LinuxSqlServerDev
  ./scripts/run-linux.sh
CMDS
else
  cat <<'CMDS'
Bootstrap succeeded.

Run commands (external SQL Server):
  1) Update the LinuxSqlServerDev connection string in RiddleHub/Web.config with your SQL Server host/user/password.
  2) export RIDDLEHUB_CONNECTION_NAME=LinuxSqlServerDev
  3) ./scripts/run-linux.sh
CMDS
fi
