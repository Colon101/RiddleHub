# RiddleHub
Riddlehub is a simple ASP C# website made for sharing riddles :0

## Make any PR's you want
Idc any help is good help

## Arch Linux Quick Start
RiddleHub is currently an ASP.NET Web Forms (.NET Framework) application, so Linux support is legacy and constrained.

- **Chosen strategy (Option A):** Mono hosting with **Apache + mod_mono** (preferred).
- **Legacy escape hatch:** `xsp4` is available only as explicit opt-in fallback.
- **Long-term recommendation (Option B):** migrate away from Web Forms to ASP.NET Core for first-class Linux support.

> Support note: ASP.NET Web Forms is part of .NET Framework, and .NET Framework is Windows-only. For Microsoft-supported hosting, run on Windows + IIS.

### 1) Bootstrap prerequisites and restore/build
From the repository root:

```bash
./scripts/bootstrap-arch.sh
```

This script validates required tools (`mono`, `msbuild`/`xbuild`, `nuget`; plus optional `docker`), restores packages, builds the solution, and prints exact run commands.
By default it auto-installs missing Arch/AUR dependencies; disable with `RIDDLEHUB_AUTO_INSTALL=0`.

If you want to install packages first:

```bash
sudo pacman -S --needed mono mono-msbuild nuget apache
# optional legacy host fallback
yay -S xsp
# or: git clone https://aur.archlinux.org/xsp.git && cd xsp && makepkg -si
# optional SQL Server container
sudo pacman -S --needed docker
```

### 2) Start the app on Linux
Use the Linux SQL Server profile and run:

```bash
export RIDDLEHUB_CONNECTION_NAME=LinuxSqlServerDev
./scripts/run-linux.sh
```

By default, `run-linux.sh` uses `apache-mod_mono` and serves on port `8080`. Override with `PORT=5000 ./scripts/run-linux.sh`.
It also auto-installs missing host dependencies by default (`RIDDLEHUB_AUTO_INSTALL=0` to disable).
When `RIDDLEHUB_CONNECTION_NAME=LinuxSqlServerDev`, it also auto-starts Docker daemon and auto-creates/starts the `riddlehub-sql` container.

To explicitly allow legacy `xsp4` fallback:

```bash
RIDDLEHUB_ALLOW_LEGACY_XSP=1 RIDDLEHUB_SERVER=xsp4 ./scripts/run-linux.sh
```

### 3) Database profile notes
Connection profiles are in `RiddleHub/Web.config`:

- `WindowsDevLocalDb`: existing Windows LocalDB + `App_Data/db.mdf`
- `LinuxSqlServerDev`: SQL Server login for Linux dev (container or external SQL Server)

If you use Docker, `bootstrap-arch.sh` prints a ready-to-run `docker run` command for SQL Server.
