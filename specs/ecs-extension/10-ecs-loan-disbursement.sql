-- ECS Loan Disbursement (spec 10 §9) — menu row to apply out of band. No schema change is needed.
-- The nav menu builds the href from form_name via ToLower().Trim().Replace(" ", "-"), so
-- 'ECS Loan Disbursement' → /ecs-loan-disbursement. It sits next to submenu 11005066 ("Loan Sanction",
-- form_name 'ecs-loan-sanction-list') under menu 11005018 ("Thrift Society Loan", role 'All').
-- The menu is filtered by role only: non-ECS users with the role see the item and hit the page's ECS gate.

-- Inspect first
SELECT submenu_id, menu_id, submenu_name, form_name, brcode, voc_status, submenu_delete
FROM   menu_sub WHERE menu_id = 11005018 ORDER BY submenu_id;

-- Insert once (guarded). Id follows the branch-prefixed MAX+1 convention.
INSERT INTO menu_sub (submenu_id, menu_id, submenu_name, form_name, submenu_delete, brcode, voc_status, submenu_icon)
SELECT (SELECT MAX(submenu_id) + 1 FROM menu_sub WHERE brcode = '11005'),
       11005018, 'Loan Disbursement', 'ECS Loan Disbursement', false, '11005', 'V', 'oi oi-document'
WHERE NOT EXISTS (SELECT 1 FROM menu_sub
                  WHERE brcode = '11005' AND lower(trim(form_name)) = 'ecs loan disbursement' AND NOT submenu_delete);

-- Revert:
-- DELETE FROM menu_sub WHERE brcode = '11005' AND lower(trim(form_name)) = 'ecs loan disbursement';
