-- Run on each intended database after reviewing submenu 11005036.
-- The menu is role-filtered at menu_main, so this submenu must belong to an
-- Admin or PlatformOnboarding main menu before a user-management link is added.
BEGIN;

DO $$
DECLARE
    parent_role text;
    branch_code text;
    other_role text;
    other_main_id integer;
BEGIN
    SELECT m.role, s.brcode INTO parent_role, branch_code
    FROM menu_sub s JOIN menu_main m ON m.menu_id = s.menu_id
    WHERE s.submenu_id = 11005036 AND NOT s.submenu_delete AND s.voc_status = 'V';

    IF parent_role IS NULL OR parent_role NOT IN ('Admin', 'PlatformOnboarding') THEN
        RAISE EXCEPTION 'Submenu 11005036 must be active under an Admin or PlatformOnboarding menu';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM menu_forms
                   WHERE submenu_id = 11005036 AND form_name = 'user-management') THEN
        INSERT INTO menu_forms (form_id, submenu_id, form_menu, form_name,
                                form_delete, brcode, voc_status, form_icon)
        VALUES ((SELECT coalesce(max(form_id), 0) + 1 FROM menu_forms),
                11005036, 'User management', 'user-management',
                false, branch_code, 'V', 'bi bi-people');
    ELSE
        UPDATE menu_forms SET form_delete = false, voc_status = 'V'
        WHERE submenu_id = 11005036 AND form_name = 'user-management';
    END IF;

    -- The same page must also be reachable from the other authorised role's menu.
    -- Use a separate submenu so an existing leaf link is not turned into a group.
    other_role := CASE parent_role WHEN 'Admin' THEN 'PlatformOnboarding' ELSE 'Admin' END;
    SELECT menu_id INTO other_main_id FROM menu_main
    WHERE role = other_role AND NOT menu_delete AND voc_status = 'V'
    ORDER BY menu_id LIMIT 1;
    IF other_main_id IS NOT NULL AND NOT EXISTS (
        SELECT 1 FROM menu_sub WHERE menu_id = other_main_id
        AND form_name = 'user-management' AND NOT submenu_delete AND voc_status = 'V') THEN
        INSERT INTO menu_sub (submenu_id, menu_id, submenu_name, form_name,
                              submenu_delete, brcode, voc_status, submenu_icon)
        VALUES ((SELECT coalesce(max(submenu_id), 0) + 1 FROM menu_sub),
                other_main_id, 'User management', 'user-management',
                false, branch_code, 'V', 'bi bi-people');
    END IF;
END $$;

COMMIT;
