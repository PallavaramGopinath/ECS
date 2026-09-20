# 05 — Draught Loan (ECS-exclusive)

> See [02-loan-module-design.md](./02-loan-module-design.md). `LoanCategory.DraughtLoan = 12`.

**Status:** stub — to be specified.

## Foundation already in place
- `Loan_Type = 12` reserved as `LoanCategory.DraughtLoan`.
- Visible only to ECS sessions via `LoanCategoryPolicy`.
- Creatable as a `Loan_Schemes` product through the existing LoanProducts admin UI.

## To decide when specifying this loan type
- Confirm intended meaning of "Draught" loan for ECS and its business rules.
- Engine reuse vs. bespoke (special interest/moratorium handling?).
- Eligibility, repayment schedule, ROI rules.
- UI fields for `Loan_Type == 12`; new pages vs. reused components.
- MIS/reports.
