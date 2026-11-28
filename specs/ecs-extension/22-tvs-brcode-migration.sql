-- TVS only: change the operational tenant code from 34-063-1429 to 1429.
-- Run against a restored copy first, after a durable backup and ID/account-number audit.
-- The separate 11005 test branch is deliberately untouched.
-- This script also widens and remaps TVS scheme keys and their relational references.
-- It leaves non-scheme primary keys and account numbers unchanged.

BEGIN;
SET LOCAL lock_timeout = '5s';

-- Reject accidental application to a different database or a repeated run.
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM public.gen_bank_name WHERE brcode = '34-063-1429') THEN
        RAISE EXCEPTION 'TVS society master with the old BrCode was not found';
    END IF;
END $$;

-- TVS's imported scheme IDs are 10 digits (1429 + a six-digit suffix).
-- Keep the suffix as a seven-digit running number under the four-digit society prefix.
CREATE TEMP TABLE tvs_loan_scheme_map AS
SELECT scheme_id AS old_id, (1429::bigint * 10000000 + scheme_id % 1000000) AS new_id
FROM public.loan_schemes WHERE brcode = '34-063-1429';
CREATE UNIQUE INDEX ON tvs_loan_scheme_map (old_id);
CREATE UNIQUE INDEX ON tvs_loan_scheme_map (new_id);

CREATE TEMP TABLE tvs_td_scheme_map AS
SELECT tdscheme_id AS old_id, (1429::bigint * 10000000 + tdscheme_id % 1000000) AS new_id
FROM public.termdeposit_schemes WHERE brcode = '34-063-1429';
CREATE UNIQUE INDEX ON tvs_td_scheme_map (old_id);
CREATE UNIQUE INDEX ON tvs_td_scheme_map (new_id);

CREATE TEMP TABLE tvs_sbca_scheme_map AS
SELECT scheme_id AS old_id, (1429::bigint * 10000000 + scheme_id % 1000000) AS new_id
FROM public.sbca_schemes WHERE brcode = '34-063-1429';
CREATE UNIQUE INDEX ON tvs_sbca_scheme_map (old_id);
CREATE UNIQUE INDEX ON tvs_sbca_scheme_map (new_id);

DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM tvs_loan_scheme_map WHERE old_id NOT BETWEEN 1429000000 AND 1429999999)
       OR EXISTS (SELECT 1 FROM tvs_td_scheme_map WHERE old_id NOT BETWEEN 1429000000 AND 1429999999)
       OR EXISTS (SELECT 1 FROM tvs_sbca_scheme_map WHERE old_id NOT BETWEEN 1429000000 AND 1429999999) THEN
        RAISE EXCEPTION 'A TVS scheme ID does not have the expected imported 1429 prefix';
    END IF;
    IF (SELECT count(*) FROM tvs_loan_scheme_map) <> 14
       OR (SELECT count(*) FROM tvs_td_scheme_map) <> 6
       OR (SELECT count(*) FROM tvs_sbca_scheme_map) <> 1 THEN
        RAISE EXCEPTION 'TVS scheme counts changed since preflight';
    END IF;
END $$;

-- These are the only database FKs to the scheme tables in the inspected ECS schema.
ALTER TABLE public.loan_scheme_deduction_rules
    DROP CONSTRAINT loan_scheme_deduction_rules_deductable_scheme_id_fkey,
    DROP CONSTRAINT loan_scheme_deduction_rules_scheme_id_fkey;

ALTER TABLE public.loan_schemes ALTER COLUMN scheme_id TYPE bigint;
ALTER TABLE public.loan_master ALTER COLUMN scheme_id TYPE bigint;
ALTER TABLE public.loan_roi_template ALTER COLUMN scheme_id TYPE bigint;
ALTER TABLE public.loan_sanction ALTER COLUMN scheme_id TYPE bigint;
ALTER TABLE public.loan_scheme_deduction_rules
    ALTER COLUMN scheme_id TYPE bigint,
    ALTER COLUMN deductable_scheme_id TYPE bigint;
ALTER TABLE public.mem_collection_priority ALTER COLUMN scheme_id TYPE bigint;
ALTER TABLE public.mem_demand_master ALTER COLUMN scheme_id TYPE bigint;
ALTER TABLE public.loan_sanction_trn ALTER COLUMN recscheme_id TYPE bigint;

ALTER TABLE public.termdeposit_schemes ALTER COLUMN tdscheme_id TYPE bigint;
ALTER TABLE public.termdeposit_master ALTER COLUMN tdscheme_id TYPE bigint;
ALTER TABLE public.termdeposit_roi_template ALTER COLUMN tdscheme_id TYPE bigint;

ALTER TABLE public.sbca_schemes ALTER COLUMN scheme_id TYPE bigint;
ALTER TABLE public.sbca_master ALTER COLUMN scheme_id TYPE bigint;

UPDATE public.loan_master t SET scheme_id = m.new_id FROM tvs_loan_scheme_map m
WHERE t.brcode = '34-063-1429' AND t.scheme_id = m.old_id;
UPDATE public.loan_roi_template t SET scheme_id = m.new_id FROM tvs_loan_scheme_map m
WHERE t.brcode = '34-063-1429' AND t.scheme_id = m.old_id;
UPDATE public.loan_sanction t SET scheme_id = m.new_id FROM tvs_loan_scheme_map m
WHERE t.brcode = '34-063-1429' AND t.scheme_id = m.old_id;
UPDATE public.loan_scheme_deduction_rules t SET scheme_id = m.new_id FROM tvs_loan_scheme_map m
WHERE t.brcode = '34-063-1429' AND t.scheme_id = m.old_id;
UPDATE public.loan_scheme_deduction_rules t SET deductable_scheme_id = m.new_id FROM tvs_loan_scheme_map m
WHERE t.brcode = '34-063-1429' AND t.deductable_scheme_id = m.old_id;
UPDATE public.mem_collection_priority t SET scheme_id = m.new_id FROM tvs_loan_scheme_map m
WHERE t.brcode = '34-063-1429' AND t.scheme_id = m.old_id;
UPDATE public.mem_demand_master t SET scheme_id = m.new_id FROM tvs_loan_scheme_map m
WHERE t.brcode = '34-063-1429' AND t.scheme_id = m.old_id;
UPDATE public.loan_sanction_trn t SET recscheme_id = m.new_id FROM tvs_loan_scheme_map m
WHERE t.brcode = '34-063-1429' AND t.recscheme_id = m.old_id;
UPDATE public.loan_schemes t SET scheme_id = m.new_id FROM tvs_loan_scheme_map m
WHERE t.brcode = '34-063-1429' AND t.scheme_id = m.old_id;

UPDATE public.termdeposit_master t SET tdscheme_id = m.new_id FROM tvs_td_scheme_map m
WHERE t.brcode = '34-063-1429' AND t.tdscheme_id = m.old_id;
UPDATE public.termdeposit_roi_template t SET tdscheme_id = m.new_id FROM tvs_td_scheme_map m
WHERE t.brcode = '34-063-1429' AND t.tdscheme_id = m.old_id;
UPDATE public.termdeposit_schemes t SET tdscheme_id = m.new_id FROM tvs_td_scheme_map m
WHERE t.brcode = '34-063-1429' AND t.tdscheme_id = m.old_id;

UPDATE public.sbca_master t SET scheme_id = m.new_id FROM tvs_sbca_scheme_map m
WHERE t.brcode = '34-063-1429' AND t.scheme_id = m.old_id;
UPDATE public.sbca_schemes t SET scheme_id = m.new_id FROM tvs_sbca_scheme_map m
WHERE t.brcode = '34-063-1429' AND t.scheme_id = m.old_id;

ALTER TABLE public.loan_scheme_deduction_rules
    ADD CONSTRAINT loan_scheme_deduction_rules_deductable_scheme_id_fkey
        FOREIGN KEY (deductable_scheme_id) REFERENCES public.loan_schemes (scheme_id),
    ADD CONSTRAINT loan_scheme_deduction_rules_scheme_id_fkey
        FOREIGN KEY (scheme_id) REFERENCES public.loan_schemes (scheme_id);

DO $$
DECLARE
    item record;
    old_count bigint;
    new_count bigint;
    changed_count bigint;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM public.societies WHERE society_code = 1429) THEN
        RAISE EXCEPTION 'Society Code 1429 is missing from public.societies';
    END IF;
    IF (SELECT count(*) FROM public.societies WHERE society_code = 1429) <> 1 THEN
        RAISE EXCEPTION 'Society Code 1429 is not unique';
    END IF;

    FOR item IN
        SELECT c.table_schema, c.table_name, c.column_name
        FROM information_schema.columns c
        JOIN information_schema.tables t
          ON t.table_schema = c.table_schema AND t.table_name = c.table_name
        WHERE c.table_schema = 'public'
          AND t.table_type = 'BASE TABLE'
          AND lower(c.column_name) = 'brcode'
        ORDER BY c.table_name
    LOOP
        EXECUTE format('SELECT count(*) FROM %I.%I WHERE %I = $1',
                       item.table_schema, item.table_name, item.column_name)
            INTO old_count USING '34-063-1429';
        EXECUTE format('SELECT count(*) FROM %I.%I WHERE %I = $1',
                       item.table_schema, item.table_name, item.column_name)
            INTO new_count USING '1429';
        IF new_count > 0 THEN
            RAISE EXCEPTION 'Target BrCode 1429 already has % rows in %.%',
                new_count, item.table_schema, item.table_name;
        END IF;
        IF old_count > 0 THEN
            EXECUTE format('UPDATE %I.%I SET %I = $1 WHERE %I = $2',
                           item.table_schema, item.table_name, item.column_name,
                           item.column_name)
                USING '1429', '34-063-1429';
            GET DIAGNOSTICS changed_count = ROW_COUNT;
            IF changed_count <> old_count THEN
                RAISE EXCEPTION 'Row count changed during migration in %.%',
                    item.table_schema, item.table_name;
            END IF;
            RAISE NOTICE '%.%: % rows', item.table_schema, item.table_name, changed_count;
        END IF;
    END LOOP;
END $$;

-- The imported directory currently has no unique constraint on this stable key.
CREATE UNIQUE INDEX IF NOT EXISTS societies_society_code_uq
    ON public.societies (society_code);

CREATE TABLE IF NOT EXISTS public.society_operational_codes (
    society_code integer PRIMARY KEY REFERENCES public.societies (society_code),
    brcode varchar(11) NOT NULL UNIQUE,
    activated_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO public.society_operational_codes (society_code, brcode)
VALUES (1429, '1429')
ON CONFLICT (society_code) DO UPDATE SET brcode = EXCLUDED.brcode;

COMMIT;
