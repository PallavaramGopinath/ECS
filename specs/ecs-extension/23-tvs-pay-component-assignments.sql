-- Populate TVS payroll assignments from the migrated Access pay_all_ded and pay_slip tables.
-- Pay_Components.xlsx maps All_Id 1/2/3/4,5 to HRA/CCA/MA/SOC. Its component IDs
-- differ from the eight component IDs already stored for TVS, so resolve by code.
-- Run once after backing up ECS. The transaction rejects a partial/repeated import.

BEGIN;
SET LOCAL lock_timeout = '5s';

DO $$
BEGIN
    IF (SELECT count(*) FROM public.pay_components WHERE brcode = '1429') <> 8
       OR (SELECT count(DISTINCT component_code) FROM public.pay_components WHERE brcode = '1429') <> 8
       OR EXISTS (
           SELECT 1 FROM (VALUES ('BP'), ('DA'), ('HRA'), ('CCA'), ('MA'), ('PF'), ('VPF'), ('SOC')) AS expected(code)
           WHERE NOT EXISTS (SELECT 1 FROM public.pay_components c
                             WHERE c.brcode = '1429' AND c.component_code = expected.code)
       ) THEN
        RAISE EXCEPTION 'TVS payroll components are incomplete or ambiguous';
    END IF;
    IF EXISTS (SELECT 1 FROM public.pay_component_assignments WHERE brcode = '1429')
       OR EXISTS (SELECT 1 FROM public.pay_component_assignments
                  WHERE id BETWEEN 14290000001 AND 14299999999) THEN
        RAISE EXCEPTION 'TVS payroll assignments already exist';
    END IF;
    IF (SELECT count(*) FROM public.pay_all_ded WHERE brcode = '1429') <> 29
       OR (SELECT count(*) FROM public.pay_all_ded
           WHERE brcode = '1429' AND NOT all_delete AND all_asscurr) <> 12 THEN
        RAISE EXCEPTION 'TVS Access allowance/deduction source changed';
    END IF;
    IF (SELECT count(*) FROM public.users WHERE brcode = '1429') <> 1 THEN
        RAISE EXCEPTION 'Expected one TVS user for assignment audit fields';
    END IF;
END $$;

CREATE TEMP TABLE tvs_pay_assignment_source ON COMMIT DROP AS
WITH access_map(all_id, component_code) AS (
    VALUES (14290000001::numeric, 'HRA'),
           (14290000002::numeric, 'CCA'),
           (14290000003::numeric, 'MA'),
           (14290000004::numeric, 'SOC'),
           (14290000005::numeric, 'SOC')
), deduped_access AS (
    -- Access repeats four employee/component/effective-date combinations.
    -- The newest Pay_Ad_Id is the authoritative row for each combination.
    SELECT DISTINCT ON (a.mem_id, m.component_code, a.all_wef::date)
           a.mem_id AS employee_id, m.component_code,
           coalesce(a.all_amount, 0)::double precision AS assigned_value,
           a.all_wef::date AS effective_date,
           (NOT a.all_delete AND a.all_asscurr) AS is_active,
           coalesce(a.all_percentage, 0)::double precision AS percentage,
           coalesce(a.all_maxamount, 0)::double precision AS maximum_amount,
           'pay_all_ded'::text AS source
    FROM public.pay_all_ded a
    JOIN access_map m ON m.all_id = a.all_id
    WHERE a.brcode = '1429'
    ORDER BY a.mem_id, m.component_code, a.all_wef::date, a.pay_ad_id DESC
), latest_slips AS (
    SELECT DISTINCT ON (p.mem_id)
           p.mem_id AS employee_id, p.pay_basic, p.pay_da_earned,
           p.pay_da_percent, p.pay_pf, p.pay_vpf,
           make_date(i.pay_year, i.pay_month, 1) AS effective_date,
           p.pay_id
    FROM public.pay_slip p
    JOIN public.pay_init i ON i.pay_id = p.pay_id AND i.brcode = '1429'
    WHERE p.brcode = '1429' AND NOT p.pay_delete AND NOT i.pay_delete
          AND p.pay_basic > 0
    ORDER BY p.mem_id, i.pay_year DESC, i.pay_month DESC,
             p.pay_id DESC, p.payslip_id DESC
), slip_assignments AS (
    SELECT p.employee_id, values_for_component.component_code,
           values_for_component.assigned_value::double precision AS assigned_value,
           p.effective_date, true AS is_active,
           values_for_component.percentage::double precision AS percentage,
           0::double precision AS maximum_amount,
           'pay_slip'::text AS source
    FROM latest_slips p
    CROSS JOIN LATERAL (VALUES
        ('BP', p.pay_basic, 0::double precision),
        ('DA', p.pay_da_earned, p.pay_da_percent),
        ('PF', p.pay_pf, 0::double precision),
        ('VPF', p.pay_vpf, 0::double precision)
    ) AS values_for_component(component_code, assigned_value, percentage)
)
SELECT s.employee_id, c.component_id, s.component_code,
       s.assigned_value, s.effective_date, s.is_active,
       s.percentage, s.maximum_amount, s.source
FROM (SELECT * FROM deduped_access UNION ALL SELECT * FROM slip_assignments) s
JOIN public.pay_components c ON c.brcode = '1429' AND c.component_code = s.component_code;

DO $$
BEGIN
    IF (SELECT count(*) FROM tvs_pay_assignment_source) <> 33
       OR (SELECT count(*) FROM tvs_pay_assignment_source WHERE is_active) <> 24
       OR (SELECT count(DISTINCT employee_id) FROM tvs_pay_assignment_source) <> 3
       OR EXISTS (
           SELECT 1 FROM tvs_pay_assignment_source s
           LEFT JOIN public.mem_master m ON m.mem_id = s.employee_id AND m.brcode = '1429'
           WHERE m.mem_id IS NULL
       )
       OR EXISTS (
           SELECT 1 FROM tvs_pay_assignment_source
           GROUP BY employee_id, component_id, effective_date HAVING count(*) > 1
       ) THEN
        RAISE EXCEPTION 'TVS payroll assignment source failed count, employee, or uniqueness checks';
    END IF;
    IF (SELECT count(DISTINCT employee_id) FROM tvs_pay_assignment_source
        WHERE source = 'pay_slip' AND effective_date = DATE '2026-10-01') <> 3 THEN
        RAISE EXCEPTION 'Latest TVS payslips are not October 2026 for all three employees';
    END IF;
END $$;

-- Without this flag the payroll calculator's DA base is zero for all TVS staff.
UPDATE public.pay_components
SET is_da_applicable = true
WHERE brcode = '1429' AND component_code = 'BP';

INSERT INTO public.pay_component_assignments
    (id, employee_id, component_id, assigned_value, effective_date, end_date,
     is_overridden, created_by, created_date, is_active, brcode, percentage, maximum_amount)
SELECT 14290000000::numeric + row_number() OVER (
           ORDER BY s.employee_id, s.component_id, s.effective_date, s.source
       ),
       s.employee_id, s.component_id, s.assigned_value, s.effective_date,
       NULL::date, false, (SELECT id FROM public.users WHERE brcode = '1429'),
       CURRENT_TIMESTAMP, s.is_active, '1429', s.percentage, s.maximum_amount
FROM tvs_pay_assignment_source s;

DO $$
BEGIN
    IF (SELECT count(*) FROM public.pay_component_assignments WHERE brcode = '1429') <> 33
       OR (SELECT count(*) FROM public.pay_component_assignments
           WHERE brcode = '1429' AND is_active) <> 24
       OR EXISTS (
           SELECT 1 FROM public.pay_component_assignments a
           JOIN public.pay_components c ON c.component_id = a.component_id
           WHERE a.brcode = '1429' AND c.brcode <> '1429'
       ) THEN
        RAISE EXCEPTION 'TVS payroll assignment post-insert verification failed';
    END IF;
END $$;

SELECT source, count(*) AS rows, count(*) FILTER (WHERE is_active) AS active_rows
FROM tvs_pay_assignment_source GROUP BY source ORDER BY source;

COMMIT;
