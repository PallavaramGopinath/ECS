# 01 — Institution-Type Modeling

> See [00-context.md](./00-context.md) for background and [02-loan-module-design.md](./02-loan-module-design.md) for the loan-module decision that builds on this.

## Decision

**No new entity is needed.** Institution type already exists in the data model and already flows
through the session. The ECS extension reuses it.

| Concern | Existing mechanism |
|---|---|
| Stored source of truth | `Gen_Bank_Name.Bank_Type` (int), one row per society (keyed by `BrCode`) — `Infin8.Coapp.Models/Gen_Bank_Name.cs` |
| Loaded at login | `UserHandler.GetAppStateAsync` → `AppState.SocietyType` — `Infin8.Coapp.BusinessLogic/Concrete/UserHandler.cs` |
| Session container | `AppState.SocietyType` (int) — `Infin8.Coapp.Utility/AppState.cs` |

## Value mapping

> **Superseded by [07-loan-type-society-type.md](./07-loan-type-society-type.md) §2.1.** The earlier
> mapping (`2` = ECS) was wrong; ECS is `3` and `2` is Cooperative Housing Society. ECS rows with
> `bank_type = 2` must be updated to `3` in the same release.

| `Bank_Type` | Society |
|---|---|
| `1` | Cooperative Primary Agriculture and Rural Development Bank (PCARDB) |
| `2` | Cooperative Housing Society |
| `3` | Cooperative Thrift & Credit Society (ECS) |
| `0` / other | Unknown. Treated as PCARDB so that existing PCARDB visibility never regresses. |

## What this extension added

1. **Typed enum** replacing the bare int — `Infin8.Coapp.Utility/InstitutionType.cs`
   (namespace `Infin8.Coapp.Utility.Enums`):
   `InstitutionType { Unknown = 0, PCARDB = 1, Housing = 2, ECS = 3 }`, plus
   `int.ToInstitutionType()` extension.

2. **Client availability via the auth claim pipeline** (the same path `BrCode` travels), so Blazor
   WASM pages can gate ECS-only UI without a separate API round-trip:
   - `UserInfoDto.SocietyType` (new field) — `Infin8.Coapp.Dto/UserInfoDto.cs`
   - `api/Auth/me` populates it from `IGeneralHandler.GetSocietyData(brCode).Bank_Type`
     (defensive: failure ⇒ `0`/Unknown) — `Infin8.Coapp.UI/Controllers/AuthController.cs`
   - `CookieAuthStateProvider` adds a `"SocietyType"` claim —
     `Infin8.Coapp.UI.Client/Providers/CookieAuthStateProvider.cs`
   - `Utilities.GetUserInfoDtoFromASP` parses the `"SocietyType"` claim —
     `Infin8.Coapp.Utility/Utilities.cs`

   > **Deliberately NOT changed:** `JwtService.GenerateAccessToken` and the login/refresh token
   > generation. The society lookup happens in `api/Auth/me` instead, so the live login path and the
   > JWT signature are untouched. This keeps the change additive and low-risk for production PCARDB.

## Safety / backward compatibility

- `Bank_Type` stays a stored int; `Gen_Bank_Name` schema is unchanged.
- Unknown society type is treated as PCARDB, so existing PCARDB visibility never regresses if the
  lookup fails or a society has no type set.

## Open follow-ups

- Confirm every live society actually has `Bank_Type` populated (PCARDB rows should be `1`).
- Decide whether server-side handlers/repositories should also branch on institution type for the new
  loan products, or whether scheme configuration alone suffices (see `02`).
