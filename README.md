# RiddleHub
Riddlehub is a simple ASP C# website made for sharing riddles :0

## Make any PR's you want
Idc any help is good help

## Arch Linux Quick Start
RiddleHub is currently an ASP.NET Web Forms (.NET Framework) application, so Linux support is legacy and constrained.

- **Chosen strategy (Option A):** Mono hosting with **Apache + mod_mono** (preferred), plus **xsp4** as a legacy fallback.
- **Long-term recommendation (Option B):** migrate away from Web Forms to ASP.NET Core for first-class Linux support.

### 1) Bootstrap prerequisites and restore/build
From the repository root:

```bash
./scripts/bootstrap-arch.sh
```

This script validates required tools (`mono`, `msbuild`/`xbuild`, `nuget`; plus optional `docker`), restores packages, builds the solution, and prints exact run commands.

### 2) Start the app on Linux
Use the Linux SQL Server profile and run:

```bash
export RIDDLEHUB_CONNECTION_NAME=LinuxSqlServerDev
./scripts/run-linux.sh
```

By default, `run-linux.sh` auto-detects host stack (`apache-mod_mono` first, otherwise `xsp4`) and serves on port `8080`. Override with `PORT=5000 ./scripts/run-linux.sh`.

### 3) Database profile notes
Connection profiles are in `RiddleHub/Web.config`:

- `WindowsDevLocalDb`: existing Windows LocalDB + `App_Data/db.mdf`
- `LinuxSqlServerDev`: SQL Server login for Linux dev (container or external SQL Server)

If you use Docker, `bootstrap-arch.sh` prints a ready-to-run `docker run` command for SQL Server.
