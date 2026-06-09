# Beautix — Deployment & Configuration

ASP.NET Core 8 MVC app, PostgreSQL (via Npgsql), deployed on Railway with Docker.

## Required environment variables (Railway → service → Variables)

| Variable | Required | Notes |
|---|---|---|
| `SUPABASE_CONNECTION` | Yes | PostgreSQL connection string in **Npgsql key/value** format, e.g. `Host=...;Port=5432;Database=postgres;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true`. **Not** a `postgresql://...` URL. |
| `ADMIN_EMAIL` | For admin login | Email of the seeded platform administrator. |
| `ADMIN_PASSWORD` | For admin login | Admin password. Must meet the policy: 8+ chars, at least one uppercase and one digit. |

Behaviour:
- If `SUPABASE_CONNECTION` is missing (and no local `DefaultConnection`), the app logs a clear error and refuses to start.
- If `ADMIN_EMAIL` / `ADMIN_PASSWORD` are missing, the app still starts but skips admin seeding (logs a warning).

## Why the site went blank before
On startup the app applies EF Core migrations. The Supabase database had become unreachable
("tenant/user not found" — a paused or deleted Supabase project), so startup threw an unhandled
exception and Railway's restart loop (10 retries) left the service down → blank page.

Startup is now wrapped in try/catch, so the app keeps serving database-independent pages
(e.g. the landing page) even when the database is temporarily unavailable.

## Redeploy checklist
1. **Restore the database** (the actual blocker):
   - Supabase dashboard → resume the paused project, **or** create a new project, **or**
   - add Railway's own PostgreSQL plugin (does not auto-pause).
2. Copy the **Npgsql-format** connection string (see table above).
3. Railway → service → Variables: set `SUPABASE_CONNECTION`, `ADMIN_EMAIL`, `ADMIN_PASSWORD`.
4. Trigger a redeploy (push to the deployed branch, or Railway → Deploy).
5. Watch the deploy logs for `Database migrated and seeded successfully.`

## Security note (important)
A real database password and admin password were previously committed to this **public** repo
(in `appsettings.json` and `Program.cs`) and they remain in the git history. **Rotate the database
password and choose a new admin password** — anyone who saw the history may know the old ones.
Optionally scrub history with `git filter-repo` / BFG, but rotation is the essential step.

## Local development
- Do **not** put secrets in `appsettings.json`. Use user-secrets:
  ```
  dotnet user-secrets init
  dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;Port=5432;Database=postgres;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true"
  ```
  …or set the `SUPABASE_CONNECTION` environment variable.
- Run with `dotnet run`.
