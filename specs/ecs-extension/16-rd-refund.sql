-- Spec 16 (ECS RD Refund, acc 10). Run on every ECS database. Idempotent.

-- §2.1: the account row's component names were misspelt ("RecurringDepsit_*").
UPDATE account_transactions
SET component_name = 'RecurringDeposit_Refund', view_component_name = 'RecurringDeposit_Refund_View'
WHERE acc_id = 10;

-- §15.3 (optional): an RD pre-closure rate, TD_Type 4, per branch. Without it no foreclosure rate is deducted.
-- Set the rate and wef the society uses before running.
-- INSERT INTO termdeposit_fctemplate (tdfc_id, td_type, tdfc_wef, tdfc_roi, tdfc_delete, usr_id, yr_id, brcode)
-- SELECT MAX(tdfc_id) + 1, 4, DATE '2000-01-01', 2, false, MAX(usr_id), MAX(yr_id), '11005'
-- FROM termdeposit_fctemplate WHERE brcode = '11005'
--   AND NOT EXISTS (SELECT 1 FROM termdeposit_fctemplate WHERE td_type = 4 AND NOT tdfc_delete AND brcode = '11005');
