-- Add 'enrollment_status' column with default if not exists
ALTER TABLE huser.consumer
ADD COLUMN IF NOT EXISTS enrollment_status VARCHAR(50) NOT NULL DEFAULT 'UNKNOWN';

-- Add 'enrollment_status_source' column with default if not exists
ALTER TABLE huser.consumer
ADD COLUMN IF NOT EXISTS enrollment_status_source VARCHAR(50) NOT NULL DEFAULT 'UNKNOWN';

-- Add constraints conditionally
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 
        FROM pg_constraint 
        WHERE conname = 'chk_enrollment_status'
    ) THEN
        ALTER TABLE huser.consumer
        ADD CONSTRAINT chk_enrollment_status 
        CHECK (enrollment_status IN ('UNKNOWN', 'ENROLLED', 'ENROLLMENT_CANCELLED', 'ENROLLMENT_TERMINATED', 'ADMIN_TERMINATED'));
    END IF;

    IF NOT EXISTS (
        SELECT 1 
        FROM pg_constraint 
        WHERE conname = 'chk_enrollment_status_source'
    ) THEN
        ALTER TABLE huser.consumer
        ADD CONSTRAINT chk_enrollment_status_source 
        CHECK (enrollment_status_source IN ('UNKNOWN', 'ONBOARDING_FLOW', 'ELIGIBILITY_FILE', 'ADMIN'));
    END IF;
END $$;
