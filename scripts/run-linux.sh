#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
APP_DIR="$ROOT_DIR/RiddleHub"

fail() {
  echo "ERROR: $*" >&2
  exit 1
}

command -v xsp4 >/dev/null 2>&1 || fail "xsp4 is required to run ASP.NET Web Forms on Linux. Install mono-xsp."

export RIDDLEHUB_CONNECTION_NAME="${RIDDLEHUB_CONNECTION_NAME:-LinuxSqlServerDev}"
PORT="${PORT:-8080}"

echo "Starting RiddleHub with xsp4"
echo "  App directory: $APP_DIR"
echo "  Port: $PORT"
echo "  Connection profile: $RIDDLEHUB_CONNECTION_NAME"

echo "Tip: Ensure your SQL Server is reachable and the schema is created before using the app."

exec xsp4 --port "$PORT" --root "$APP_DIR"
