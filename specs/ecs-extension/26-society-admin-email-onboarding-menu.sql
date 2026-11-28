-- Run against the intended test database after reviewing the account and menu rows.
-- The existing account keeps its branch and Admin role. This script adds a separate
-- platform role and a menu entry. It does not send email or create society Admins.
BEGIN;

DO $$
DECLARE
    account_count integer;
    main_id integer;
    sub_id integer;
BEGIN
    SELECT count(*) INTO account_count
    FROM users WHERE lower(email) = 'gopulu.nath@gmail.com';
    IF account_count <> 1 THEN
        RAISE EXCEPTION 'Expected one test account, found %', account_count;
    END IF;

    UPDATE users
    SET role = CASE
            WHEN 'PlatformOnboarding' = ANY(string_to_array(coalesce(role, ''), ',')) THEN role
            WHEN nullif(trim(role), '') IS NULL THEN 'PlatformOnboarding'
            ELSE role || ',PlatformOnboarding'
        END,
        is_active = true,
        updated_at = now()
    WHERE lower(email) = 'gopulu.nath@gmail.com';

    SELECT menu_id INTO main_id FROM menu_main
    WHERE trim(menu_name) = 'Platform onboarding' AND role = 'PlatformOnboarding'
    ORDER BY menu_id LIMIT 1;
    IF main_id IS NULL THEN
        SELECT coalesce(max(menu_id), 0) + 1 INTO main_id FROM menu_main;
        INSERT INTO menu_main (menu_id, menu_name, menu_delete, brcode, voc_status, menu_icon, role)
        VALUES (main_id, 'Platform onboarding', false, '11005', 'V', 'bi bi-person-plus', 'PlatformOnboarding');
    END IF;

    SELECT submenu_id INTO sub_id FROM menu_sub
    WHERE menu_id = main_id AND trim(form_name) = 'society-admin-onboarding'
    ORDER BY submenu_id LIMIT 1;
    IF sub_id IS NULL THEN
        SELECT coalesce(max(submenu_id), 0) + 1 INTO sub_id FROM menu_sub;
        INSERT INTO menu_sub (submenu_id, menu_id, submenu_name, form_name,
                              submenu_delete, brcode, voc_status, submenu_icon)
        VALUES (sub_id, main_id, 'Society Admin onboarding', 'society-admin-onboarding',
                false, '11005', 'V', 'bi bi-envelope-plus');
    ELSE
        UPDATE menu_sub SET submenu_delete = false, voc_status = 'V'
        WHERE submenu_id = sub_id;
    END IF;
END $$;

COMMIT;
