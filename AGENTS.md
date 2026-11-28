# Project working context

This repository is the shared PCARDB and ECS cooperative banking application. Read
`CLAUDE.md` before changing code; it contains the detailed architecture, conventions,
current ECS flows, and known traps. For ECS behavior, read the relevant file in
`specs/ecs-extension/`, especially the later decision/departure sections. The early
`00-context.md` describes the original problem and contains questions that later specs
have resolved.

## Where changes belong

- `Infin8.Coapp.UI.Client`: interactive Blazor pages and components. Client code calls
  the server over HTTP.
- `Infin8.Coapp.UI`: the live host, controllers, dependency injection, and RDLC reports.
- `Infin8.Coapp.BusinessLogic`: handlers and business rules.
- `Infin8.Coapp.Repository`: EF Core context, repositories, and unit of work.
- `Infin8.Coapp.Models`, `.Dto`, `.Utility`: entities, transfer types, and shared helpers.
- `Infin8.Coapp.API` is an older parallel host outside the solution. Do not use it as
  the default location for new endpoints or registration.

## Starting a change

1. Check `git status` and preserve existing work. Some local untracked files may be
   present; do not remove them without the owner's request.
2. Read the relevant current code and spec. Check for existing patterns before adding
   a new vertical slice.
3. Run `dotnet build Infin8.Coapp.sln` to establish the local baseline. The SDK is
   pinned in `global.json` to .NET 10.0.300 with minor roll-forward. There is no test
   project or CI workflow in this repository.
4. For UI changes, the runnable host is `Infin8.Coapp.UI`; verify affected behavior in
   the app when a suitable PostgreSQL development database is available. A build alone
   does not verify banking calculations or database-dependent flows.

## Git workflow

- `origin` is `https://github.com/PallavaramGopinath/ECS.git`. Future development
  should be pushed to this repository.
- Keep unrelated local files out of commits. Record the branch, commit, and push
  result when reporting completed work.

## Data and deployment constraints

- PostgreSQL schema is managed outside EF migrations. Coordinate any required SQL
  with the relevant spec's `.sql` file and preserve compatibility with live PCARDB data.
- Branch (`BrCode`) filtering is explicit, as are business date and financial year
  values. Financial transaction logic must use the branch business day.
- ECS account flows often stage Maker input and recompute figures on Checker approval.
  Preserve both sides of that workflow and the shared calculation rules.
- Configuration files may contain credentials. Do not copy their values into docs,
  logs, or replies.
