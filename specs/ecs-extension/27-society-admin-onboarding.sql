-- Apply to the ECS database before using the Society Admin onboarding page.
-- Additive columns preserve existing PCARDB and migrated TVS users.
ALTER TABLE public.users
    ADD COLUMN IF NOT EXISTS must_change_password boolean NOT NULL DEFAULT false,
    ADD COLUMN IF NOT EXISTS invitation_expires_at timestamp without time zone,
    ADD COLUMN IF NOT EXISTS security_stamp text;

CREATE TABLE IF NOT EXISTS public.society_admin_onboarding (
    society_id bigint PRIMARY KEY REFERENCES public.societies(society_id),
    brcode text NOT NULL UNIQUE,
    admin_name text NOT NULL,
    admin_email text NOT NULL,
    agreement_reference text NOT NULL,
    agreement_date timestamp without time zone NOT NULL,
    email_verified_at timestamp without time zone NOT NULL,
    user_id numeric UNIQUE,
    status text NOT NULL DEFAULT 'Ready',
    last_attempt_at timestamp without time zone,
    sent_at timestamp without time zone,
    last_error text,
    updated_by numeric NOT NULL,
    CONSTRAINT society_admin_onboarding_status_check
        CHECK (status IN ('Ready', 'Sending', 'Sent', 'Failed'))
);

CREATE UNIQUE INDEX IF NOT EXISTS society_admin_onboarding_email_unique
    ON public.society_admin_onboarding (lower(admin_email));

CREATE TABLE IF NOT EXISTS public.society_admin_invitation_audit (
    audit_id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    society_id bigint NOT NULL REFERENCES public.societies(society_id),
    user_id numeric,
    actor_id numeric NOT NULL,
    event text NOT NULL,
    occurred_at timestamp without time zone NOT NULL,
    recipient text
);
