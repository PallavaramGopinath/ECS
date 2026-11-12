# 03 — Surety Loan (ECS-exclusive)

> See [02-loan-module-design.md](./02-loan-module-design.md). Per [07-loan-type-society-type.md](./07-loan-type-society-type.md),
> this product is a `Loan_Schemes` row under `Loan_Type 1` (`LoanCategory.ThriftCreditLoans`), not a
> separate loan type; `10` is retired.

**Status:** stub — to be specified.

## Foundation already in place
- Stored as `Loan_Type = 1` (Thrift & Credit loans); identified by its `Scheme_Id`.
- `Loan_Type 1` is visible only to ECS sessions via `LoanCategoryPolicy`.
- Product-specific behaviour comes from rule columns on `loan_schemes` (spec 07 §3.4), never from
  the scheme's id or name. When specifying this product, list its rules as a table of
  `loan_schemes` column → value, marking any column that has to be added.
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
