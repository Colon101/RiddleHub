# Arch Linux Quick Start

RiddleHub is currently an ASP.NET Web Forms (.NET Framework) application, so Linux support is legacy and constrained.

- **Chosen strategy (Option A):** Mono hosting with **Apache + mod_mono** (preferred).
- **Legacy escape hatch:** `xsp4` is available only as explicit opt-in fallback.
- **Long-term recommendation (Option B):** migrate away from Web Forms to ASP.NET Core for first-class Linux support.

> Note: the upstream `mono/xsp` repository is archived, so this project treats `xsp4` as dev-only fallback rather than the primary Linux host.
>
> Support note: ASP.NET Web Forms is part of .NET Framework, and .NET Framework is Windows-only. For Microsoft-supported hosting, run on Windows + IIS.

## 1) Bootstrap prerequisites and restore/build
From the repository root:

```bash
./scripts/bootstrap-arch.sh
```

This script validates required tools (`mono`, `msbuild`/`xbuild`, `nuget`) and ensures at least one Linux host is available:
- `apachectl` with `mono_module` loaded (**preferred**)

It can also use `xsp4` as an explicit legacy fallback only when `RIDDLEHUB_ALLOW_LEGACY_XSP=1`.

It also restores packages, builds the solution, and prints exact run commands.
By default it auto-installs missing Arch/AUR dependencies; disable with `RIDDLEHUB_AUTO_INSTALL=0`.

Install packages directly (if needed):

```bash
sudo pacman -S --needed mono mono-msbuild nuget apache
# optional legacy host fallback
yay -S xsp
# or: git clone https://aur.archlinux.org/xsp.git && cd xsp && makepkg -si
# optional SQL Server container
sudo pacman -S --needed docker
```

Optional: set `RIDDLEHUB_APACHE_CONF=/etc/httpd/conf/extra/riddlehub.conf` so bootstrap/run can verify your Apache vhost file exists.

## 2) Start the app on Linux
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

# force xsp4 fallback (legacy opt-in)
RIDDLEHUB_ALLOW_LEGACY_XSP=1 RIDDLEHUB_SERVER=xsp4 PORT=8080 ./scripts/run-linux.sh
```

`run-linux.sh` also auto-installs missing host dependencies by default (`RIDDLEHUB_AUTO_INSTALL=0` to disable).
With `RIDDLEHUB_CONNECTION_NAME=LinuxSqlServerDev`, `run-linux.sh` also auto-starts Docker daemon and auto-creates/starts SQL container `riddlehub-sql`.

## 3) Database profile notes
Connection profiles are in `RiddleHub/Web.config`:

- `WindowsDevLocalDb`: existing Windows LocalDB + `App_Data/db.mdf`
- `LinuxSqlServerDev`: SQL Server login for Linux dev (container or external SQL Server)

If you use Docker, `bootstrap-arch.sh` prints a ready-to-run `docker run` command for SQL Server.

## 4) Apache vhost template
A starter config is included at `scripts/apache-riddlehub.conf.example`. Copy and adapt paths/user as needed.
