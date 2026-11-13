-- ECS Loan Sanction (spec 06) — additive DDL / data to apply out of band.
-- The entities Loan_Eligibility and Mem_RetirementAge already exist in Infin8.Coapp.Models (with DbSets),
-- but their tables were found missing in a local database. Every statement is guarded, so it is safe to
-- run against a database that already has them. Column names are the lowercased C# property names
-- (CSISContext.UseLowerCaseTableAndColumnNames). IDs are assigned by the application (MAX+1): no sequences.

-- 1. loan_eligibility — REQUIRED: every sanction save writes one row (loan_sanction.loanelig_id).
CREATE TABLE IF NOT EXISTS loan_eligibility (
    loanelig_id                 integer          NOT NULL PRIMARY KEY,
    loanelig_date               timestamp without time zone NULL,
    loan_id                     numeric          NOT NULL DEFAULT 0,
    mem_id                      numeric          NOT NULL DEFAULT 0,
    grosspay                    double precision NOT NULL DEFAULT 0,
    societydeductions           double precision NOT NULL DEFAULT 0,
    otherthansocietydeductions  double precision NOT NULL DEFAULT 0,
    totaldeductions             double precision NOT NULL DEFAULT 0,
    netpay                      double precision NOT NULL DEFAULT 0,
    totalprlos                  double precision NOT NULL DEFAULT 0,
    totalintos                  double precision NOT NULL DEFAULT 0,
    sharecapitaldue             double precision NOT NULL DEFAULT 0,
    sharecapitalsuritydue       double precision NOT NULL DEFAULT 0,
    thriftdepositdue            double precision NOT NULL DEFAULT 0,
    entrancefeedue              double precision NOT NULL DEFAULT 0,
    feedues                     double precision NOT NULL DEFAULT 0,
    otherdues                   double precision NOT NULL DEFAULT 0,
    twentyfivetimesofgp         double precision NOT NULL DEFAULT 0,
    fiftypercentongp            double precision NOT NULL DEFAULT 0,
    twentyfivepercentongp       double precision NOT NULL DEFAULT 0,
    percentageofnetpay          double precision NOT NULL DEFAULT 0,
    demandamt                   double precision NOT NULL DEFAULT 0,
    netpayafterdemand           double precision NOT NULL DEFAULT 0,
    loanappliedamt              double precision NOT NULL DEFAULT 0,
    disbursedamt                double precision NOT NULL DEFAULT 0,
    periodofloan                integer          NOT NULL DEFAULT 0,
    ismemberbecomedefaulter     boolean          NOT NULL DEFAULT false,
    issuritybecomedefaulter     boolean          NOT NULL DEFAULT false,
    loanelig_delete             boolean          NOT NULL DEFAULT false,
    voc_id                      numeric          NOT NULL DEFAULT 0,
    usr_id                      numeric          NOT NULL DEFAULT 0,
    yr_id                       numeric          NOT NULL DEFAULT 0,
    brcode                      text             NULL
);

-- 2. mem_retirementage — OPTIONAL: per-member retirement age. Without it the code falls back to
--    map_general.tsisretirementageinmonths. RetirementAge is read as months; values below 150 are
--    treated as years.
CREATE TABLE IF NOT EXISTS mem_retirementage (
    mem_id                integer NOT NULL PRIMARY KEY,
    retirementage         integer NOT NULL DEFAULT 0,
    retirementage_delete  boolean NOT NULL DEFAULT false,
    usr_id                integer NOT NULL DEFAULT 0,
    yr_id                 integer NOT NULL DEFAULT 0,
    brcode                text    NULL
);

-- 3. Office Note report row — REQUIRED for Print Office Note. Replace the placeholders.
--    The controller looks the row up by ReportName = 'Office Note'.
-- INSERT INTO reports_master (report_id, reportgrp_id, reportname, reportfilename, reportdelete,
--                             firstsignature, secondsignature, thirdsignature, brcode)
-- SELECT COALESCE(MAX(report_id), 0) + 1, <report group id>, 'Office Note', 'OfficeNote.rdlc', false,
--        '<first signature>', '<second signature>', '<third signature>', '<brcode>'
-- FROM reports_master
-- WHERE NOT EXISTS (SELECT 1 FROM reports_master WHERE reportname = 'Office Note');

-- 4. Schemes. The page lists only loan_schemes with loan_type 10 (Surety), 11 (Education) or 12 (Draught).
--    Configure them through the Loan Products screen, or re-type an existing ECS scheme, e.g.:
-- UPDATE loan_schemes SET loan_type = 10 WHERE scheme_id = <id> AND brcode = '<brcode>';

-- loan_eligibility.mem_id must be numeric like mem_master.mem_id: ECS member ids (e.g. 110050001301)
-- exceed the integer range, so saving a sanction overflowed. Widening is backward compatible for
-- existing (PCARDB) rows. No-op when the column is already numeric.
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.columns
               WHERE table_name = 'loan_eligibility' AND column_name = 'mem_id' AND data_type = 'integer') THEN
        ALTER TABLE loan_eligibility ALTER COLUMN mem_id TYPE numeric;
    END IF;
END $$;

-- loan_eligibility.loan_id, voc_id, usr_id and yr_id must be numeric too: ECS user/year/loan ids
-- (e.g. usr_id 110050000001) exceed the integer range, so saving a sanction overflowed. Widening is
-- backward compatible for existing rows. Each column is a no-op when already numeric.
DO $$
DECLARE c text;
BEGIN
    FOREACH c IN ARRAY ARRAY['loan_id', 'voc_id', 'usr_id', 'yr_id'] LOOP
        IF EXISTS (SELECT 1 FROM information_schema.columns
                   WHERE table_name = 'loan_eligibility' AND column_name = c AND data_type = 'integer') THEN
            EXECUTE format('ALTER TABLE loan_eligibility ALTER COLUMN %I TYPE numeric', c);
        END IF;
    END LOOP;
END $$;
