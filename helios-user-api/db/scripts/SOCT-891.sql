-- Add new enrollment status "DEACTIVATED" to chk_enrollment_status contraint
-- Step 1: Drop the old CHECK constraint
ALTER TABLE huser.consumer
DROP CONSTRAINT chk_enrollment_status;

-- Step 2: Add the updated CHECK constraint with 'DEACTIVATED'
ALTER TABLE huser.consumer
ADD CONSTRAINT chk_enrollment_status 
CHECK (
    enrollment_status IN (
        'UNKNOWN',
        'ENROLLED',
        'ENROLLMENT_CANCELLED',
        'ENROLLMENT_TERMINATED',
        'ADMIN_TERMINATED',
        'DEACTIVATED'
    )
);