-- Menu entries for the report pages added in the TSIS report port. Report pages hang off the "Reports"
-- menu as menu_sub rows whose form_name is the page route (NavMenu turns it into the link).
-- Apply to every ECS database. Works per branch (each branch has its own "Reports" menu row) and is
-- idempotent: it only switches on / inserts what is missing.
--
--   /reports-loan               <- revives the "Loan Reports" placeholder
--   /reports-certificate        <- revives the "IT Certificate for Member" placeholder
--   /reports-income-tax         <- revives the "IT Reports for Society" placeholder
--   /reports-personal-register  <- new "Personal Register"
--   /utility-emi-schedule       <- new "Loan Repayment Schedule"

DO $$
DECLARE
    r       record;
    next_id integer;
BEGIN
    FOR r IN SELECT menu_id, brcode FROM menu_main
             WHERE trim(menu_name) = 'Reports' AND NOT menu_delete AND voc_status = 'V'
    LOOP
        -- placeholders that already exist for these pages
        UPDATE menu_sub SET form_name = 'reports-loan', submenu_delete = false, voc_status = 'V'
         WHERE menu_id = r.menu_id AND trim(submenu_name) = 'Loan Reports';
        UPDATE menu_sub SET form_name = 'reports-certificate', submenu_delete = false, voc_status = 'V'
         WHERE menu_id = r.menu_id AND trim(submenu_name) = 'IT Certificate for Member';
        UPDATE menu_sub SET form_name = 'reports-income-tax', submenu_delete = false, voc_status = 'V'
         WHERE menu_id = r.menu_id AND trim(submenu_name) = 'IT Reports for Society';

        -- new rows; ids follow the branch-prefixed MAX+1 numbering the existing rows use
        IF NOT EXISTS (SELECT 1 FROM menu_sub WHERE menu_id = r.menu_id AND trim(form_name) = 'reports-personal-register') THEN
            SELECT COALESCE(MAX(submenu_id), 0) + 1 INTO next_id FROM menu_sub WHERE brcode IS NOT DISTINCT FROM r.brcode;
            INSERT INTO menu_sub (submenu_id, menu_id, submenu_name, form_name, submenu_delete, brcode, voc_status, submenu_icon)
            VALUES (next_id, r.menu_id, 'Personal Register', 'reports-personal-register', false, r.brcode, 'V', 'oi oi-document');
        END IF;
        IF NOT EXISTS (SELECT 1 FROM menu_sub WHERE menu_id = r.menu_id AND trim(form_name) = 'utility-emi-schedule') THEN
            SELECT COALESCE(MAX(submenu_id), 0) + 1 INTO next_id FROM menu_sub WHERE brcode IS NOT DISTINCT FROM r.brcode;
            INSERT INTO menu_sub (submenu_id, menu_id, submenu_name, form_name, submenu_delete, brcode, voc_status, submenu_icon)
            VALUES (next_id, r.menu_id, 'Loan Repayment Schedule', 'utility-emi-schedule', false, r.brcode, 'V', 'oi oi-document');
        END IF;
    END LOOP;
END $$;
