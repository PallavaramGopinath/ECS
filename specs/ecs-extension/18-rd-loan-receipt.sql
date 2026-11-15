-- Spec 18 §2.1: component names of the ECS Recurring Deposit Loan Receipt account row (acc 25).
-- Run on every ECS database, after 17-rd-loan-disbursement.sql (which sets up the Loan_Type 4 scheme and its ledgers).
-- Idempotent.

UPDATE account_transactions
SET component_name = 'RecurringDeposit_LoanReceipt',
    view_component_name = 'RecurringDeposit_LoanReceipt_View'
WHERE acc_id = 25;
