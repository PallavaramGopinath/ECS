-- Spec 17 — ECS Loan on Recurring Deposit (acc 14). Data only, no schema change. Idempotent.
-- Run on every ECS database before using the component. Ledger ids embed the branch: check them per branch
-- and change '11005' / 11005xxxxxxx below for any other branch.

-- §2.1 Account row: component names used by Member Transaction and Checkerboard.
UPDATE account_transactions
SET component_name = 'RecurringDeposit_LoanDisbursement',
    view_component_name = 'RecurringDeposit_LoanDisbursement_View'
WHERE acc_id = 14;

-- §2.2 1. Scheme 6 becomes the RD-loan scheme (Loan_Type 4, LOAN ON RD / INTEREST RECEIVED ON RD LOANS, no penal).
UPDATE loan_schemes
SET loan_type = 4,
    prlled_id = 110050000010,     -- LOAN ON RD
    intled_id = 110050000053,     -- INTEREST RECEIVED ON RD LOANS
    disb_type = 1,
    int_application = 0,           -- no demand-based application; interest is worked out on receipt (spec 18)
    pi_application = 0             -- no penal
WHERE scheme_id = 6 AND brcode = '11005';

-- §2.2 2. Rate margin over the highest RD rate (§6.2). TSIS hard-codes 2.
INSERT INTO loan_roi_template (roi_id, scheme_id, agency, wef, roi, pi, updt, roitemplate_delete, usr_id, yr_id, brcode, voc_status)
SELECT (SELECT MAX(roi_id) + 1 FROM loan_roi_template WHERE brcode = '11005'),
       6, 'S', DATE '2000-01-01', 2, 0, false, false,
       110050000001, 110050000001, '11005', 'V'
WHERE NOT EXISTS (SELECT 1 FROM loan_roi_template
                  WHERE scheme_id = 6 AND brcode = '11005' AND NOT roitemplate_delete);

-- Check: exactly one live Loan_Type 4 scheme per branch (the handler refuses zero or several).
-- SELECT scheme_id, scheme_name, loan_type, prlled_id, intled_id, loannostartwith
-- FROM loan_schemes WHERE loan_type = 4 AND NOT scheme_delete AND brcode = '11005';

-- Check: RD eligibility row (§6.1, TD_Type 2).
-- SELECT * FROM termdeposit_loaneligibletemplate WHERE td_type = 2 AND NOT td_delete AND brcode = '11005';
