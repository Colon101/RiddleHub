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

mono_module_loaded() {
  has_cmd apachectl && apachectl -M 2>/dev/null | grep -qi 'mono_module'
}

export RIDDLEHUB_CONNECTION_NAME="${RIDDLEHUB_CONNECTION_NAME:-LinuxSqlServerDev}"
PORT="${PORT:-8080}"
RIDDLEHUB_SERVER="${RIDDLEHUB_SERVER:-auto}"

if [[ "$RIDDLEHUB_SERVER" == "auto" ]]; then
  if mono_module_loaded; then
    RIDDLEHUB_SERVER="apache-mod_mono"
  elif has_cmd xsp4; then
    RIDDLEHUB_SERVER="xsp4"
  else
    fail "No supported host found. Configure Apache with mod_mono (preferred) or install xsp4 (legacy fallback)."
  fi
fi

echo "Starting RiddleHub"
echo "  App directory: $APP_DIR"
echo "  Port: $PORT"
echo "  Connection profile: $RIDDLEHUB_CONNECTION_NAME"
echo "  Host stack: $RIDDLEHUB_SERVER"

echo "Tip: Ensure your SQL Server is reachable and schema is created before using the app."

case "$RIDDLEHUB_SERVER" in
  apache-mod_mono)
    has_cmd apachectl || fail "apachectl is required for apache-mod_mono mode."
    mono_module_loaded || fail "Apache mono_module is not loaded. Enable mod_mono in Apache first."
    echo "Using Apache + mod_mono. Ensure your Apache vhost points to: $APP_DIR"
    exec apachectl -DFOREGROUND
    ;;
  xsp4)
    has_cmd xsp4 || fail "xsp4 is required for xsp4 mode. Install mono-xsp."
    echo "WARN: xsp4 is a legacy fallback (mono/xsp upstream is archived). Prefer apache-mod_mono when possible."
    exec xsp4 --port "$PORT" --root "$APP_DIR"
    ;;
  *)
    fail "Invalid RIDDLEHUB_SERVER='$RIDDLEHUB_SERVER'. Use: auto, apache-mod_mono, or xsp4."
    ;;
esac
