# RiddleHub

RiddleHub is a small ASP.NET Web Forms site for sharing riddles.

## Local development

ASP.NET Web Forms targets .NET Framework. Windows with IIS is the supported
deployment path; the Mono/Apache scripts are a best-effort local development
option.

On Arch Linux, restore and build from the repository root:

```bash
./scripts/bootstrap-arch.sh
```

Before starting the Linux SQL profile, supply unique secrets. The scripts do
not contain fallback passwords:

```bash
read -rsp "SQL Server password: " RIDDLEHUB_SQL_PASSWORD; echo
export RIDDLEHUB_SQL_PASSWORD
read -rsp "RiddleHub admin password: " RIDDLEHUB_ADMIN_PASSWORD; echo
export RIDDLEHUB_ADMIN_PASSWORD
export RIDDLEHUB_CONNECTION_NAME=LinuxSqlServerDev
./scripts/run-linux.sh
```

The local application and Docker SQL listener bind to `127.0.0.1` by default.
The app creates a named `RiddleHub` database and initializes its schema; the
repository does not ship a database containing accounts.

Connection profiles are in `RiddleHub/Web.config`:

- `WindowsDevLocalDb` creates/uses a named LocalDB database.
- `LinuxSqlServerDev` obtains its password from `RIDDLEHUB_SQL_PASSWORD` and is
  intended only for the loopback development container.

See [the Arch Linux quick start](docs/ARCH_LINUX_QUICKSTART.md) for host setup
and [the secure deployment guide](docs/SECURE_DEPLOYMENT.md) before exposing the
site beyond a developer workstation.
