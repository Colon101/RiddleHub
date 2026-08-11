# Secure deployment

RiddleHub is an ASP.NET Web Forms application. For Microsoft-supported hosting,
deploy it on Windows with IIS. The Mono scripts in this repository are intended
for local development and bind both the application and Docker SQL Server ports
to loopback.

## Secrets

Set unique `RIDDLEHUB_ADMIN_PASSWORD` and `RIDDLEHUB_SQL_PASSWORD` values in the
service manager or secret store. There are no repository defaults. If a local
`.env` file is needed, place it in the repository root (the parent of the
`RiddleHub/` web root), set its permissions to `0600`, and keep it untracked.
`RIDDLEHUB_ENV_FILE` may point to another file outside the web root. The app
refuses to read an admin `.env` located beneath the web root, and Web.config and
the Apache example block dotfiles as defense in depth.

## HTTPS

Do not expose the development HTTP listener directly. Terminate TLS 1.2 or newer
at IIS or a maintained reverse proxy, redirect HTTP to HTTPS, and proxy to the
loopback-only application listener. Use a valid certificate for the public host.
The Release transform requires secure cookies and adds HSTS; only enable HSTS
after HTTPS works for the complete host.

For production SQL Server, use `Encrypt=True;TrustServerCertificate=False` and a
server certificate whose name and chain validate. The checked-in Linux profile
uses `TrustServerCertificate=True` only for the loopback development container.
Restrict the database firewall to the application host and use a least-privilege
database login instead of `sa`.

## Data migration

The application creates the named `RiddleHub` database and schema when needed;
no MDF/LDF data files are shipped. Existing accounts whose password field is not
in the `pbkdf2-sha256` format are marked for reset. A correct legacy password is
accepted once, immediately replaced with PBKDF2, and the user must choose a new
password before an authenticated session is issued. Password changes and account
renames increment a server-side session version, revoking older sessions.
