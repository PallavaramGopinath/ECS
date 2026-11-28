# Society Admin email onboarding

**Status:** Draft for review, 28 November 2026. This document proposes future work only; it does not authorize sending email, changing accounts, or applying database changes.

## 1. Goal

After an agreement with an ECS society, a platform administrator selects that society and sends its designated administrator a unique user ID and a randomly generated temporary password. At first login, the administrator must change the password before using the application. The society Admin can then create its own named Maker and Checker users. The initial test sender is `gopulu.nath@gmail.com`; a dedicated application mailbox will replace it later through configuration.

This flow covers individual, explicitly selected societies. It must not automatically contact all 1,637 directory entries.

## 2. Existing position and dependencies

- `public.societies` contains 1,637 imported directory rows (`21-societies-directory-import.md`). It has no operational `BrCode`, administrator name, or administrator email, and is not yet connected to login. The proposed combined society code is not a `BrCode`.
- `Gen_Bank_Name` has `BrCode`, `Bank_Type`, and `Bank_Email`; `Bank_Email` is not proof that it belongs to the authorised administrator. `Users` has an email and `BrCode` but no first-login password-change flag.
- Current registration endpoints accept a caller-supplied role and set `BrCode = "11001"`. The current login/refresh path lacks required user-status checks. The multi-society access plan (`19-multi-society-login-and-access-plan.md`) identifies these and other tenant-isolation issues as rollout blockers.
- The client uses HTTP controllers in the live `Infin8.Coapp.UI` host. New server actions belong there and in the handler/repository layers, not in the older `Infin8.Coapp.API` project.

**Release gate:** complete the relevant identity, tenant-isolation, and account-status controls in plan 19 before enabling onboarding for real societies. A pilot may use test accounts and agreed test recipients while these changes are under development.

## 3. Onboarding data and authority

For each society, record a reviewed one-to-one mapping from `societies.society_id` and its permanent `society_code` to the operational `Gen_Bank_Name.BrCode`. Verify that the target has ECS `Bank_Type`, is active, and has its business day, financial year, and required master data ready. Do not derive `BrCode` from the source serial number or proposed combined code.

Record the agreement status, agreement reference/date, authorised Admin's name, designated Admin email, who verified that email and when, onboarding status, and latest send attempt. A blank, unverified, malformed, or duplicate destination must block sending. A society's generic mailbox can be used only if the agreement explicitly identifies it as the Admin's delivery address. Confirm any change to the destination through the platform operator before re-sending credentials.

Only an authenticated **platform onboarding administrator** may use this page and its APIs. This is distinct from a society Admin and must be enforced on the server. The server resolves the selected society, agreement, recipient, and `BrCode` from trusted records; a browser-supplied role, recipient, or `BrCode` cannot grant access or redirect a credential email. Audit who changed the verified recipient and who authorised each send.

The first Admin must be a named individual, with one globally unique login ID and one active society assignment. The platform administrator assigns role `Admin` on the server. A society Admin may create Maker and Checker accounts only for its own `BrCode`; creating additional Admins requires platform approval, as recommended in plan 19. A society Admin must not gain Maker/Checker approval rights merely from its Admin role.

## 4. Page design

Add a **Society Admin onboarding** page visible only to the platform onboarding role. The main selector is a searchable, scrollable dropdown with a checkbox beside each of the 1,637 society names. Show society code, region/circle, mapped `BrCode`, agreement status, Admin name, and a masked destination email to distinguish similarly named societies. Load/search in pages or virtualise the list so the full directory remains usable.

- Search by society name, society code, region/circle, and `BrCode`. Filters for *Ready*, *Already sent*, *Pending*, and *Needs attention* help operators work in batches.
- Checkboxes select individual societies. `Select all shown` affects only the current filtered result, and the page displays the exact selected count. Selection is retained while moving between result pages and can be cleared explicitly.
- A preflight review lists every selected society, full verified destination email, proposed or existing Admin login ID, and readiness result. Rows with missing agreement, mapping, recipient, or account conflicts are excluded from sending and display the reason.
- The operator confirms the exact eligible count and recipients before pressing **Send invitations**. There is no automatic send on selection or page load.
- Show per-society `Sent`, `Failed`, `Pending`, or `Needs review`, with attempted time, sender account, and a safe error summary. Do not display temporary passwords in results, browser storage, logs, exports, or audit entries. Allow export of a status report without secrets.
- Default to a small configurable batch size and rate limit; display progress. Prevent double submission, but allow an authorised, deliberate retry of failed or expired invitations.

## 5. Provisioning and delivery flow

1. The platform administrator confirms the agreement, correct society mapping, recipient, and readiness. The server performs these checks again for every selected society at submission time.
2. For a society without an Admin, create a unique login ID and a cryptographically random temporary password using a secure random generator. Set the Admin role and `BrCode` on the server. Store only the password hash, following the application's approved password hashing policy. Add a persistent `must_change_password` state and invitation expiry/status. Never use a common password across societies.
3. Queue one independent delivery job per society. Send each message only to its verified recipient; do not use a shared `To`, `Cc`, or `Bcc` list. The email contains the application URL, society name/code, login ID, temporary password, expiry, and help contact. It must not contain financial or member data. Use TLS for mail transport.
4. Record an audit event for creation, send attempt, provider acceptance, failure, retry, activation, password change, and recipient change. Store message/provider IDs where available, without storing the temporary password or full message body. Provider acceptance means *sent to provider*, not confirmed delivery or reading.
5. A failed or uncertain delivery does not create another Admin. An authorised retry rotates the temporary password, invalidates the previous one, and sends a new message to the current verified recipient. Expired invitations follow the same reset/reissue process. Handle partial batches independently and make retries idempotent at the account level.

The one-time plaintext password has to exist briefly to construct the outgoing email. Keep that processing server-side, avoid durable plaintext storage, and clear in-memory copies where practical. If the chosen mail queue cannot securely carry it without persistence, use a short-lived single-use activation link instead and revise the message template. The selected delivery method must be reviewed before implementation.

## 6. First login and later user management

On login, check the password, `Is_Active`, `Is_Account_Closed`, `Is_Locked`, invitation expiry, and active society mapping before issuing a normal session. A valid temporary password leads only to a **Change password** screen. Enforce the same restriction on every server endpoint and on refresh tokens, so changing a URL or calling an API directly cannot bypass it.

Require the current temporary password, a new password, and confirmation. Enforce the approved password policy and reject reuse of the temporary password. After a successful change, clear `must_change_password`, invalidate the temporary credential and all earlier sessions/refresh tokens, and issue a fresh normal session. Expired or lost credentials require a platform-controlled reissue to the verified recipient. Do not put passwords or reset tokens in URLs.

After activation, the society Admin gets a user-management page scoped to its own `BrCode`. It can create named Maker and Checker accounts, disable them, and initiate a controlled reset; each new user must also change a temporary password on first login. The server, not the client form, assigns the role and society. Maker and Checker operations must retain the separation-of-duties rules in plan 19.

## 7. Mail configuration and operational limits

Use `gopulu.nath@gmail.com` as the **test sender address only**. Keep provider credentials and any application-specific password in secret configuration, never in source, SQL, this document, or logs. Configure sender address/display name, SMTP or approved mail provider, transport security, timeout, batch rate, and reply-to address separately so the dedicated application mailbox can replace the test address without code changes. Verify the Gmail account's allowed authentication method, sending limits, and sender policy during implementation before any pilot; do not assume it can deliver a 1,637-recipient campaign.

Test first with an explicitly agreed test society and a controlled recipient, inspect the email and first-login path, then expand in small batches. Do not send real credentials until mapping, recipient verification, server authorization, session restriction, and tenant-isolation acceptance gates pass.

## 8. Acceptance criteria

1. The selector shows all 1,637 directory societies with search, checkboxes, selection count, and accurate readiness states; filtering and pagination do not change the chosen set unexpectedly.
2. A society without an approved agreement, verified Admin email, active ECS mapping/`BrCode`, or ready financial setup cannot be sent credentials; the reason is visible in preflight.
3. A society Admin or unauthenticated caller cannot access the page or invoke the send/provision APIs. A forged society ID, recipient, role, or `BrCode` cannot redirect delivery or create cross-society access.
4. Two selected societies receive separate messages, distinct login IDs and passwords, and accounts bound to their respective `BrCode`. No other society is contacted.
5. Temporary-password login can reach only password change; direct URL/API and refresh-token attempts cannot reach normal pages. Changing the password enables normal Admin access and revokes earlier sessions.
6. A duplicate click, timeout, failed send, or retry does not create a second Admin. A retry invalidates the older temporary password and records the outcome without leaking secrets.
7. A society Admin can create Maker and Checker users only for its own society and cannot self-assign platform privileges or approve its own work.
8. The pilot proves email receipt, first-login change, failure recovery, audit history, and switching the configured sender to the eventual application mailbox. Existing PCARDB login and user flows remain intact.

## 9. Decisions for review

1. What record establishes a society's agreement, and who verifies its Admin contact and email? This is required before any real send.
2. Should the initial email carry a temporary password as requested, or should it use a single-use activation link if secure queued delivery of a temporary password is impractical?
3. What are the temporary password/invitation expiry, password policy, and support contact shown in the message?
4. Which exact platform role and named operators may create the first Admin and approve reissues?
5. What is the pilot application URL and test recipient, and what dedicated sender mailbox will replace `gopulu.nath@gmail.com`?

## 10. Suggested email text

**Subject:** ECS application access for {SocietyName}

> Dear {AdminName},
>
> Following your society's agreement, access to the ECS application has been prepared for {SocietyName} ({SocietyCode}).
>
> Application: {ApplicationUrl}  
> User ID: {UserId}  
> Temporary password: {TemporaryPassword}  
> Expires: {ExpiryDateTimeWithTimeZone}
>
> Please sign in and change the password immediately. Until you do, other application functions are unavailable. After activation, you can create named Maker and Checker users for your society.
>
> If you did not expect this message, contact {SupportContact}. Do not reply with your password.
>
> ECS application support

If the activation-link option is selected, replace the temporary-password line with the single-use link and its expiry.

## Related specifications

- `19-multi-society-login-and-access-plan.md` — identity, role, tenant-isolation, and rollout gates.
- `21-societies-directory-import.md` and `21-societies-directory.sql` — 1,637-row directory and its present fields.
