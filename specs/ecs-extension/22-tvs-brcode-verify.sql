-- Run after 22-tvs-brcode-migration.sql on the rehearsal and target databases.
-- Raises on any old operational BrCode or broken TVS scheme reference.

DO $$
DECLARE
    item record;
    remaining bigint;
BEGIN
    FOR item IN
        SELECT c.table_schema, c.table_name, c.column_name
        FROM information_schema.columns c
        JOIN information_schema.tables t
          ON t.table_schema = c.table_schema AND t.table_name = c.table_name
        WHERE c.table_schema = 'public' AND t.table_type = 'BASE TABLE'
          AND lower(c.column_name) = 'brcode'
    LOOP
        EXECUTE format('SELECT count(*) FROM %I.%I WHERE %I = $1',
                       item.table_schema, item.table_name, item.column_name)
            INTO remaining USING '34-063-1429';
        IF remaining > 0 THEN
            RAISE EXCEPTION 'Old TVS BrCode remains in %.%: % rows',
                item.table_schema, item.table_name, remaining;
        END IF;
    END LOOP;

    IF (SELECT count(*) FROM public.loan_schemes
        WHERE brcode = '1429' AND scheme_id BETWEEN 14290000001 AND 14299999999) <> 14
       OR (SELECT count(*) FROM public.termdeposit_schemes
        WHERE brcode = '1429' AND tdscheme_id BETWEEN 14290000001 AND 14299999999) <> 6
       OR (SELECT count(*) FROM public.sbca_schemes
        WHERE brcode = '1429' AND scheme_id BETWEEN 14290000001 AND 14299999999) <> 1 THEN
        RAISE EXCEPTION 'TVS scheme counts or 11-digit ID ranges are wrong';
    END IF;

    IF EXISTS (SELECT 1 FROM public.loan_master m
               WHERE m.brcode = '1429' AND NOT EXISTS
                    (SELECT 1 FROM public.loan_schemes s
                     WHERE s.brcode = '1429' AND s.scheme_id = m.scheme_id))
       OR EXISTS (SELECT 1 FROM public.loan_sanction m
                  WHERE m.brcode = '1429' AND NOT EXISTS
                       (SELECT 1 FROM public.loan_schemes s
                        WHERE s.brcode = '1429' AND s.scheme_id = m.scheme_id))
       OR EXISTS (SELECT 1 FROM public.termdeposit_master m
                  WHERE m.brcode = '1429' AND NOT EXISTS
                       (SELECT 1 FROM public.termdeposit_schemes s
                        WHERE s.brcode = '1429' AND s.tdscheme_id = m.tdscheme_id))
       OR EXISTS (SELECT 1 FROM public.sbca_master m
                  WHERE m.brcode = '1429' AND NOT EXISTS
                       (SELECT 1 FROM public.sbca_schemes s
                        WHERE s.brcode = '1429' AND s.scheme_id = m.scheme_id)) THEN
        RAISE EXCEPTION 'A TVS account or sanction has an unresolved scheme reference';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM public.society_operational_codes
                   WHERE society_code = 1429 AND brcode = '1429') THEN
        RAISE EXCEPTION 'TVS society operational-code mapping is missing';
    END IF;
END $$;

SELECT 'members' AS item, count(*) AS rows FROM public.mem_master WHERE brcode = '1429'
UNION ALL SELECT 'loans', count(*) FROM public.loan_master WHERE brcode = '1429'
UNION ALL SELECT 'vouchers', count(*) FROM public.fin_voucher WHERE brcode = '1429'
UNION ALL SELECT 'users', count(*) FROM public.users WHERE brcode = '1429';
