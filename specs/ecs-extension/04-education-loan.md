# 04 — Education Loan (ECS-exclusive)

> See [02-loan-module-design.md](./02-loan-module-design.md). `LoanCategory.EducationLoan = 11`.

**Status:** stub — to be specified.

## Foundation already in place
- `Loan_Type = 11` reserved as `LoanCategory.EducationLoan`.
- Visible only to ECS sessions via `LoanCategoryPolicy`.
- Creatable as a `Loan_Schemes` product through the existing LoanProducts admin UI.

## To decide when specifying this loan type
- Engine reuse vs. bespoke (e.g. tranche/semester-wise disbursement, moratorium during study period).
- Eligibility and documentation (course, institution, co-borrower).
- Repayment schedule, ROI rules, interest treatment during moratorium.
- UI fields for `Loan_Type == 11`; new pages vs. reused components.
- MIS/reports.
