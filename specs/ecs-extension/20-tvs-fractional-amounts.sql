-- TVS Access contains fractional payment and closing-balance values.
-- Preserve those amounts instead of rounding them into the former integer fields.
BEGIN;
SET LOCAL lock_timeout = '5s';
ALTER TABLE public.mem_payable
    ALTER COLUMN payment_amount TYPE double precision,
    ALTER COLUMN closing_balance TYPE double precision;
ALTER TABLE public.mem_trn
    ALTER COLUMN intcalc_amt TYPE double precision,
    ALTER COLUMN intpaid_amt TYPE double precision;
COMMIT;
