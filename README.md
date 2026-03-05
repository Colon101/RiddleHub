# RiddleHub
Riddlehub is a simple ASP C# website made for sharing riddles :0

## Make any PR's you want
Idc any help is good help

## Arch Linux Quick Start
RiddleHub is currently an ASP.NET Web Forms (.NET Framework) application, so Linux support is legacy and constrained.

- **Chosen strategy (Option A):** Mono hosting with **Apache + mod_mono** (preferred), plus **xsp4** as a legacy fallback.
- **Long-term recommendation (Option B):** migrate away from Web Forms to ASP.NET Core for first-class Linux support.

> Note: the upstream `mono/xsp` repository is archived, so this project treats `xsp4` as dev-only fallback rather than the primary Linux host.

### 1) Bootstrap prerequisites and restore/build
From the repository root:

```bash
./scripts/bootstrap-arch.sh
```

This script validates required tools (`mono`, `msbuild`/`xbuild`, `nuget`) and ensures at least one Linux host is available:
- `apachectl` with `mono_module` loaded (**preferred**), or
- `xsp4` (**legacy fallback**)

It also restores packages, builds the solution, and prints exact run commands.

Optional: set `RIDDLEHUB_APACHE_CONF=/etc/httpd/conf/extra/riddlehub.conf` so bootstrap/run can verify your Apache vhost file exists.

### 2) Start the app on Linux
Use the Linux SQL Server profile and run:

```bash
export RIDDLEHUB_CONNECTION_NAME=LinuxSqlServerDev
./scripts/run-linux.sh
```

Host selection options:

```bash
# auto-detect (default)
./scripts/run-linux.sh

# force Apache + mod_mono
RIDDLEHUB_APACHE_CONF=/etc/httpd/conf/extra/riddlehub.conf RIDDLEHUB_SERVER=apache-mod_mono ./scripts/run-linux.sh

# force xsp4 fallback (legacy)
RIDDLEHUB_SERVER=xsp4 PORT=8080 ./scripts/run-linux.sh
```

### 3) Database profile notes
Connection profiles are in `RiddleHub/Web.config`:

- `WindowsDevLocalDb`: existing Windows LocalDB + `App_Data/db.mdf`
- `LinuxSqlServerDev`: SQL Server login for Linux dev (container or external SQL Server)

If you use Docker, `bootstrap-arch.sh` prints a ready-to-run `docker run` command for SQL Server.


### 4) Apache vhost template
A starter config is included at `scripts/apache-riddlehub.conf.example`. Copy and adapt paths/user as needed.
