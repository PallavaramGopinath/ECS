# Project Context: Extending PCARDB System to Support ECS

> Purpose: This document gives Claude Code the background, architecture, and constraints needed to extend the existing PCARDB Blazor project so it also serves Cooperative Employees' Thrift and Credit Societies (ECS), without breaking existing PCARDB functionality.

## 1. Background

- An existing production application was built single-handedly for **Primary Cooperative Agriculture and Rural Development Banks (PCARDB)**.
- The goal now is to **extend the same codebase** (not build a separate project) so it can also serve **Cooperative Employees' Thrift and Credit Societies (ECS)**.
- PCARDB and ECS share most modules. They differ mainly in the **Loan Module**.
- This extension work is being done with Claude Code, working inside the existing PCARDB repository.

## 2. Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Blazor, .NET 10, Visual Studio 2026 |
| Server-side logic | C# |
| Queries | LINQ — Query Syntax for complex queries, Method Syntax for simple queries |
| Database | PostgreSQL v18 |

## 3. Module Inventory

### 3.1 Common Modules (PCARDB + ECS — no change needed)

| Module | Sub-modules |
|---|---|
| Member Module | Share Capital, Dividend, Sundry Creditor, Sundry Debtor |
| Accounts Module | — |
| Term Deposits | Fixed Deposits, Recurring Deposits |
| Loan on Fixed Deposits | — |
| Loan on Recurring Deposits | — |
| Jewel Loans | — |
| MIS | Per-module reporting |

### 3.2 Loan Module — Where PCARDB and ECS Diverge

| Loan Type | PCARDB | ECS |
|---|---|---|
| Jewel Loan | ✅ | ✅ |
| Loan on Fixed Deposit | ✅ | ✅ |
| Loan on Recurring Deposit | ✅ | ✅ |
| Surety Loan | ❌ | ✅ (ECS only) |
| Education Loan | ❌ | ✅ (ECS only) |
| Draught Loan | ❌ | ✅ (ECS only) |

This means the Loan Module needs to become **institution-type-aware**: PCARDB and ECS share a common loan core (jewel/FD/RD loans) but ECS adds three additional loan products that don't apply to PCARDB.

## 4. Key Architectural Question for This Extension

Before writing code, Claude Code should help decide and document an approach for:

1. **Institution-type modeling** — e.g., an `InstitutionType` enum/flag (`PCARDB`, `ECS`) attached to a Society/Branch entity, used to control which loan products and menu items are visible/available.
2. **Loan module abstraction** — whether to:
   - Extend the existing Loan entity/table with a `LoanProductType` and conditional fields, or
   - Introduce a base `Loan` abstraction with type-specific extensions (table-per-type or discriminator column) for Surety/Education/Draught loans.
3. **Shared vs exclusive UI** — how Blazor pages/menus should conditionally render ECS-only loan types without duplicating shared components.
4. **Migration safety** — since PCARDB is already live, any schema change must be additive/backward-compatible (no breaking changes to existing PCARDB loan data/queries).

## 5. Spec File Organization

```
specs/
  ecs-extension/
    00-context.md              <- this file
    01-institution-type.md     <- entity/flag design
    02-loan-module-design.md   <- abstraction decision
    03-surety-loan.md
    04-education-loan.md
    05-draught-loan.md
```

## 6. Open Questions to Resolve With Claude Code Early

- Should `InstitutionType` live on the Society/Branch record, or per-user/session context?
- Do Surety/Education/Draught loans need their own approval workflows, or do they reuse the existing loan approval pipeline?
- Are interest rate rules, repayment schedules, and MIS reports for the new loan types structurally similar enough to reuse existing components, or do they need dedicated calculation logic?
- Any existing naming conventions (table prefixes, namespace structure) Claude Code should follow for new ECS-specific entities?

---

> Note: The canonical copy of this context also lives at `D:\BlazorApps\BLAZOR_ECS_Document\00-context.md`.
> See [01-institution-type.md](./01-institution-type.md) and [02-loan-module-design.md](./02-loan-module-design.md) for the decisions taken.
