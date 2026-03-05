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

echo "==> Validating prerequisites"
need_cmd mono "Install with: sudo pacman -S mono"

MSBUILD_CMD=""
if command -v msbuild >/dev/null 2>&1; then
  MSBUILD_CMD="msbuild"
elif command -v xbuild >/dev/null 2>&1; then
  MSBUILD_CMD="xbuild"
else
  fail "Missing build tool. Install mono-msbuild or mono-xbuild (pacman package: mono-msbuild)."
fi

echo "Found build tool: $MSBUILD_CMD"
need_cmd nuget "Install with: sudo pacman -S nuget"

if command -v docker >/dev/null 2>&1; then
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

Run commands (SQL Server in Docker + Mono xsp4):
  docker run -d --name riddlehub-sql \
    -e ACCEPT_EULA=Y \
    -e MSSQL_SA_PASSWORD='Your_strong_Password123' \
    -p 1433:1433 \
    mcr.microsoft.com/mssql/server:2022-latest

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
