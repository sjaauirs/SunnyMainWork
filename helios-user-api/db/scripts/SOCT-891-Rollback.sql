-- This is rollback script for SOCT-891.sql
-- Step 1: Drop the updated CHECK constraint with 'DEACTIVATED'
ALTER TABLE huser.consumer
DROP CONSTRAINT chk_enrollment_status;

-- Step 2: Recreate the original CHECK constraint without 'DEACTIVATED'
ALTER TABLE huser.consumer
ADD CONSTRAINT chk_enrollment_status 
CHECK (
    enrollment_status IN (
        'UNKNOWN',
        'ENROLLED',
        'ENROLLMENT_CANCELLED',
        'ENROLLMENT_TERMINATED',
        'ADMIN_TERMINATED'
    )
);
