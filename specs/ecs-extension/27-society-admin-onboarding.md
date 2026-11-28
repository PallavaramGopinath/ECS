# Society administrator invitations

The platform onboarding page now lists the imported society directory, keeps selections across search pages, records an agreement and verified administrator contact, reviews readiness, and sends up to ten selected invitations per request. The Excel upload and validation flow remains a later task after application testing.

Apply `27-society-admin-onboarding.sql` to the ECS database before opening the page. This is an additive schema change. The directory import from `21-societies-directory.sql` is also required.

Configure these keys in server secret configuration or environment variables (use `__` between environment variable path parts):

- `OnboardingMail:Host` and `OnboardingMail:Port` (SMTP with STARTTLS, usually port 587)
- `OnboardingMail:Username` and `OnboardingMail:Password`
- `OnboardingMail:From` (the sender address; the agreed Gmail account may be used for a test)
- `OnboardingMail:ApplicationUrl` (the deployed login URL)
- `OnboardingMail:SupportContact`

No mail settings or credentials are stored in source control. The send action refuses to run until all keys are present. Each selected society is sent separately to the email recorded for its named administrator. The generated password is stored only as a hash in `users`; the result and audit rows contain no password. A failed send leaves the account for a controlled retry, which rotates the temporary password. An expired, unchanged invitation may also be reissued. A successful send cannot be repeated.

The first administrator is provisioned with role `Admin` and the reviewed operational `BrCode`. The invited user must change the temporary password within two days. Other API routes reject the temporary session. The change revokes refresh tokens and changes the security stamp, invalidating earlier access tokens. Old user rows retain normal login behavior through additive defaults.

Before enabling mail for real societies, run a two-society tenant isolation pilot as described in `19-multi-society-login-and-access-plan.md`; many existing endpoints still trust client-supplied branch codes. Test the SMTP message and first-login flow with an agreed test recipient. The page does not upload or validate society financial data.
