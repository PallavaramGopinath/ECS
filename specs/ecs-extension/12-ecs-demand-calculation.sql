-- Menu entry for ECS Demand Calculation (spec 12), route /ecs-demand-calculation.
-- The page hangs off the Admin-only "Thrift Society Demand" menu as a menu_sub row whose form_name is the route
-- (NavMenu turns it into the link). That menu already carries a "Demand Calculation" placeholder pointing at
-- under-development, so the script revives it; a branch without the placeholder gets a new row.
--
-- Apply to every ECS database. Works per branch, touches only ECS branches (gen_bank_name.bank_type = 3), and is
-- idempotent: it only switches on / inserts what is missing.

DO $$
DECLARE
    r       record;
    next_id integer;
BEGIN
    FOR r IN SELECT m.menu_id, m.brcode
               FROM menu_main m
               JOIN gen_bank_name b ON b.brcode = m.brcode
              WHERE trim(m.menu_name) = 'Thrift Society Demand' AND NOT m.menu_delete AND m.voc_status = 'V'
                AND b.bank_type = 3
    LOOP
        -- the existing placeholder
        UPDATE menu_sub SET form_name = 'ecs-demand-calculation', submenu_delete = false, voc_status = 'V'
         WHERE menu_id = r.menu_id AND trim(submenu_name) = 'Demand Calculation'
           AND trim(coalesce(form_name, '')) <> 'ecs-demand-calculation';

        -- no placeholder: new row; ids follow the branch-prefixed MAX+1 numbering the existing rows use
        IF NOT EXISTS (SELECT 1 FROM menu_sub WHERE menu_id = r.menu_id AND trim(form_name) = 'ecs-demand-calculation') THEN
            SELECT COALESCE(MAX(submenu_id), 0) + 1 INTO next_id FROM menu_sub WHERE brcode IS NOT DISTINCT FROM r.brcode;
            INSERT INTO menu_sub (submenu_id, menu_id, submenu_name, form_name, submenu_delete, brcode, voc_status, submenu_icon)
            VALUES (next_id, r.menu_id, 'Demand Calculation', 'ecs-demand-calculation', false, r.brcode, 'V', 'oi oi-calculator');
        END IF;
    END LOOP;
END $$;
