-- Backfill TVS pay_slip_trn.component_id from active payroll assignments.
-- Legacy rows can contain both allowance and deduction amounts. The existing
-- row is retained, component_id represents All_Id, and Ded_Id is untouched.
-- The single deleted zero-value placeholder has no allowance and stays NULL.

BEGIN;
SET LOCAL lock_timeout = '5s';

DO $$
BEGIN
    IF (SELECT count(*) FROM public.pay_slip_trn WHERE brcode = '1429') <> 790
       OR EXISTS (SELECT 1 FROM public.pay_slip_trn
                  WHERE brcode = '1429' AND component_id IS NOT NULL)
       OR (SELECT count(*) FROM public.pay_component_assignments
           WHERE brcode = '1429' AND is_active) <> 24 THEN
        RAISE EXCEPTION 'TVS payslip or assignment counts changed since preflight';
    END IF;
END $$;

CREATE TEMP TABLE tvs_pay_slip_component_map ON COMMIT DROP AS
WITH access_map(all_id, component_code) AS (
    VALUES (14290000001::numeric, 'HRA'),
           (14290000002::numeric, 'CCA'),
           (14290000003::numeric, 'MA')
)
SELECT t.pay_tr_id, a.component_id
FROM public.pay_slip_trn t
JOIN access_map m ON m.all_id = t.all_id
JOIN public.pay_component_assignments a
  ON a.employee_id = t.mem_id AND a.brcode = '1429' AND a.is_active
JOIN public.pay_components c
  ON c.component_id = a.component_id AND c.brcode = '1429'
 AND c.component_code = m.component_code
WHERE t.brcode = '1429';

DO $$
BEGIN
    IF (SELECT count(*) FROM tvs_pay_slip_component_map) <> 789
       OR (SELECT count(DISTINCT pay_tr_id) FROM tvs_pay_slip_component_map) <> 789
       OR EXISTS (
           SELECT 1 FROM public.pay_slip_trn t
           WHERE t.brcode = '1429' AND t.all_id <> 14290000000
             AND NOT EXISTS (SELECT 1 FROM tvs_pay_slip_component_map m
                             WHERE m.pay_tr_id = t.pay_tr_id)
       ) THEN
        RAISE EXCEPTION 'A TVS payslip row has no unique allowance assignment';
    END IF;
END $$;

UPDATE public.pay_slip_trn t
SET component_id = m.component_id
FROM tvs_pay_slip_component_map m
WHERE t.pay_tr_id = m.pay_tr_id AND t.brcode = '1429';

DO $$
BEGIN
    IF (SELECT count(*) FROM public.pay_slip_trn
        WHERE brcode = '1429' AND component_id IS NOT NULL) <> 789
       OR (SELECT count(*) FROM public.pay_slip_trn
           WHERE brcode = '1429' AND component_id IS NULL
             AND all_id = 14290000000 AND paytr_delete) <> 1
       OR EXISTS (
           SELECT 1 FROM public.pay_slip_trn t
           JOIN public.pay_components c ON c.component_id = t.component_id
           WHERE t.brcode = '1429' AND c.brcode <> '1429'
       ) THEN
        RAISE EXCEPTION 'TVS payslip component backfill failed verification';
    END IF;
END $$;

SELECT c.component_code, count(*) AS rows
FROM public.pay_slip_trn t
JOIN public.pay_components c ON c.component_id = t.component_id
WHERE t.brcode = '1429'
GROUP BY c.component_code ORDER BY c.component_code;

COMMIT;
