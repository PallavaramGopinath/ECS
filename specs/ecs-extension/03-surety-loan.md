# 03 — Surety Loan (ECS-exclusive)

> See [02-loan-module-design.md](./02-loan-module-design.md). `LoanCategory.SuretyLoan = 10`.

**Status:** stub — to be specified.

## Foundation already in place
- `Loan_Type = 10` reserved as `LoanCategory.SuretyLoan`.
- Visible only to ECS sessions via `LoanCategoryPolicy`.
- Creatable as a `Loan_Schemes` product through the existing LoanProducts admin UI.

## To decide when specifying this loan type
- Engine reuse vs. bespoke: does it reuse `Loan_Master` / `Loan_Trn` / repayment schedule / interest
  calc as-is, or need dedicated logic?
- Surety/guarantor modelling — note existing `Loan_Sanction.SuretyMem_Id` and
  `Loan_Members.Mem_Guarantor` already exist and may be reused.
- Eligibility (salary-based?), repayment schedule, ROI rules.
- UI: which fields show for `Loan_Type == 10` in `LoanProductsCreate.razor` and disbursement/recovery
  pages; new pages vs. reused components.
- MIS/reports.
