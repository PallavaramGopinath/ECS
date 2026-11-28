-- Society directory from ECS_Societies_Codes_Review.xlsx, sheet ECS.
-- The source BRCode column is deliberately omitted. The proposed combined
-- society code is a separate review field and is not an application BrCode.
CREATE TABLE IF NOT EXISTS public.societies (
    society_id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    source_serial_number integer NOT NULL UNIQUE,
    region text NOT NULL,
    circle text NOT NULL,
    block text NOT NULL,
    society_type text NOT NULL,
    society_name text NOT NULL,
    bank_name text NOT NULL,
    bank_account_number text NOT NULL,
    ifsc_code text NOT NULL,
    region_code integer NOT NULL,
    circle_code integer NOT NULL,
    society_code integer NOT NULL,
    proposed_combined_society_code varchar(11) NOT NULL UNIQUE,
    pattern_for_review text NOT NULL
);

COMMENT ON TABLE public.societies IS
    'Directory imported from ECS_Societies_Codes_Review.xlsx; proposed codes and banking details require review before operational use.';
COMMENT ON COLUMN public.societies.proposed_combined_society_code IS
    'Proposed RR-CCC-SSSS code from the workbook; not the application BrCode.';
