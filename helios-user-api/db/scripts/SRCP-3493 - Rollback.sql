

-- Drop CHECK constraint and column 'enrollment_status' if they exist
ALTER TABLE huser.consumer DROP CONSTRAINT IF EXISTS chk_enrollment_status;
ALTER TABLE huser.consumer DROP COLUMN IF EXISTS enrollment_status;

-- Drop CHECK constraint and column 'enrollment_status_source' if they exist
ALTER TABLE huser.consumer DROP CONSTRAINT IF EXISTS chk_enrollment_status_source;
ALTER TABLE huser.consumer DROP COLUMN IF EXISTS enrollment_status_source;
