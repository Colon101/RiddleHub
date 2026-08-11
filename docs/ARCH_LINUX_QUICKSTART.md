# Arch Linux quick start

RiddleHub is an ASP.NET Web Forms (.NET Framework) application. Linux hosting is
legacy and best-effort via Mono. Windows with IIS is the Microsoft-supported
production option.

## 1. Bootstrap prerequisites and build

From the repository root:

```bash
./scripts/bootstrap-arch.sh
```

The script validates Mono, `msbuild`/`xbuild`, NuGet, and a host. Apache with
`mod_mono` is preferred. The archived `xsp4` host is an explicit development
fallback only:

```bash
RIDDLEHUB_ALLOW_LEGACY_XSP=1 ./scripts/bootstrap-arch.sh
```

Automatic package installation can be disabled with
`RIDDLEHUB_AUTO_INSTALL=0`.

## 2. Configure secrets

Create unique secrets for each environment. No password has a repository
default:

```bash
read -rsp "SQL Server password: " RIDDLEHUB_SQL_PASSWORD; echo
export RIDDLEHUB_SQL_PASSWORD
read -rsp "RiddleHub admin password: " RIDDLEHUB_ADMIN_PASSWORD; echo
export RIDDLEHUB_ADMIN_PASSWORD
```

The admin page is disabled when `RIDDLEHUB_ADMIN_PASSWORD` is absent. For local
file-based configuration, copy `.env.example` to `.env` in the repository root,
set mode `0600`, and export its values before launching the process. Never put
`.env` below the `RiddleHub/` web root.

## 3. Start the app

```bash
export RIDDLEHUB_CONNECTION_NAME=LinuxSqlServerDev
./scripts/run-linux.sh
```

The HTTP and Docker SQL listeners bind to `127.0.0.1`. A non-loopback
`RIDDLEHUB_BIND_ADDRESS` is rejected. For Apache mode, copy and enable
`scripts/apache-riddlehub.conf.example`, then set its path:

```bash
RIDDLEHUB_APACHE_CONF=/etc/httpd/conf/extra/riddlehub.conf \
  RIDDLEHUB_SERVER=apache-mod_mono ./scripts/run-linux.sh
```

To explicitly use the legacy XSP fallback:

```bash
RIDDLEHUB_ALLOW_LEGACY_XSP=1 RIDDLEHUB_SERVER=xsp4 ./scripts/run-linux.sh
```

## 4. Database initialization

`WindowsDevLocalDb` and `LinuxSqlServerDev` use a named `RiddleHub` database.
The application creates the database/schema when the configured SQL identity is
allowed to do so. No MDF/LDF files or demo credentials are shipped. For an
existing database, legacy password rows are marked for a mandatory password
reset and migrated after the user proves knowledge of the old password.

Read [SECURE_DEPLOYMENT.md](SECURE_DEPLOYMENT.md) before any remote deployment.
