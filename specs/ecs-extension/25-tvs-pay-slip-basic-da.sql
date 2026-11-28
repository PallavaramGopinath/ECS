-- Backfill Basic Pay and DA allowance rows for every TVS pay/employee pair.
-- Prefer the active slip when Access migration left both an old deleted slip
-- and its replacement for the same pair. Preserve deleted-only history.

BEGIN;
SET LOCAL lock_timeout = '5s';
LOCK TABLE public.pay_slip_trn IN SHARE ROW EXCLUSIVE MODE;

DO $$
BEGIN
    IF (SELECT count(*) FROM public.pay_components
        WHERE brcode = '1429' AND component_id = 14290000001
          AND component_code = 'BP' AND component_type = 1) <> 1
       OR (SELECT count(*) FROM public.pay_components
        WHERE brcode = '1429' AND component_id = 14290000002
          AND component_code = 'DA' AND component_type = 1) <> 1 THEN
        RAISE EXCEPTION 'TVS Basic Pay or DA component mapping changed';
    END IF;
END $$;

CREATE TEMP TABLE tvs_basic_da_source ON COMMIT DROP AS
WITH selected_slips AS (
    SELECT DISTINCT ON (p.pay_id, p.mem_id)
           p.pay_id, p.mem_id, p.pay_basic, p.pay_da_earned,
           p.pay_delete, p.usr_id, p.yr_id, p.brcode, p.voc_status
    FROM public.pay_slip p
    WHERE p.brcode = '1429'
    ORDER BY p.pay_id, p.mem_id, p.pay_delete, p.payslip_id DESC
)
SELECT p.*, c.component_id, c.led_id,
       CASE c.component_code WHEN 'BP' THEN p.pay_basic
                             WHEN 'DA' THEN p.pay_da_earned END AS amount
FROM selected_slips p
JOIN public.pay_components c ON c.brcode = p.brcode
 AND c.component_id IN (14290000001, 14290000002);

DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM tvs_basic_da_source
        GROUP BY pay_id, mem_id, component_id HAVING count(*) <> 1
    ) OR (SELECT count(*) FROM tvs_basic_da_source) <>
         2 * (SELECT count(DISTINCT (pay_id, mem_id))
              FROM public.pay_slip WHERE brcode = '1429') THEN
        RAISE EXCEPTION 'TVS payslip source is incomplete or duplicated';
    END IF;
    IF EXISTS (
        SELECT 1 FROM public.pay_slip_trn t
        JOIN tvs_basic_da_source s ON s.brcode = t.brcode
          AND s.pay_id = t.pay_id AND s.mem_id = t.mem_id
          AND s.component_id = t.component_id
    ) THEN
        RAISE EXCEPTION 'A TVS Basic Pay or DA transaction already exists';
    END IF;
END $$;

INSERT INTO public.pay_slip_trn
    (pay_tr_id, pay_id, mem_id, all_id, ded_id, loan_id, led_id,
     all_type, ded_type, pay_component_type, all_ded_amt,
     allowance_amt, deduction_amt, paytr_delete, usr_id, yr_id,
     brcode, voc_status, component_id)
SELECT (SELECT coalesce(max(pay_tr_id), 14290000000)
        FROM public.pay_slip_trn
        WHERE pay_tr_id BETWEEN 14290000000 AND 14299999999)
         + row_number() OVER (ORDER BY s.pay_id, s.mem_id, s.component_id),
       s.pay_id, s.mem_id, s.component_id, 0, 0, s.led_id,
       1, 0, 1, 0, s.amount, 0, s.pay_delete, s.usr_id, s.yr_id,
       s.brcode, s.voc_status, s.component_id
FROM tvs_basic_da_source s;

DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM tvs_basic_da_source s
        LEFT JOIN public.pay_slip_trn t ON t.brcode = s.brcode
          AND t.pay_id = s.pay_id AND t.mem_id = s.mem_id
          AND t.component_id = s.component_id
        GROUP BY s.pay_id, s.mem_id, s.component_id, s.amount, s.pay_delete
        HAVING count(t.pay_tr_id) <> 1
            OR max(t.allowance_amt) IS DISTINCT FROM s.amount
            OR bool_or(t.paytr_delete) IS DISTINCT FROM s.pay_delete
    ) THEN
        RAISE EXCEPTION 'TVS Basic Pay and DA backfill failed verification';
    END IF;
END $$;

SELECT c.component_code, count(*) AS rows,
       count(*) FILTER (WHERE t.paytr_delete) AS deleted_rows
FROM public.pay_slip_trn t
JOIN public.pay_components c ON c.component_id = t.component_id
WHERE t.brcode = '1429' AND c.brcode = '1429'
  AND c.component_code IN ('BP', 'DA')
GROUP BY c.component_code ORDER BY c.component_code;

COMMIT;
