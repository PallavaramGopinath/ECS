# User management

The `/user-management` page allows society Admins to create, modify, deactivate,
and reactivate users in their own branch. PlatformOnboarding users can manage users
in societies whose onboarding status is `Sent`, and can manage pure
PlatformOnboarding accounts in their own branch. A society Admin cannot view or
change a PlatformOnboarding account. The API enforces these rules using the
authenticated user's database row and the onboarding mapping.

New accounts require a temporary password chosen by the operator. The operator
must deliver it through an approved channel. The user must change it within two
days. Changing an account revokes its refresh tokens and security stamp, so
existing sessions stop working. Delete is a soft close that also revokes sessions;
the page can reactivate a closed account. An operator cannot change or delete
their own account, or remove the society's last active Admin.

Apply `27-society-admin-onboarding.sql` before using this page: it supplies the
password-change columns and the onboarding mapping. Apply
`28-user-management-menu.sql` to add the requested `menu_forms` row under
`submenu_id = 11005036` and a link under the other authorised role's main menu.
Review the submenu and target database before applying the script. Test create,
login, first-password change, role edit, and deactivation against a development
database for each role.
