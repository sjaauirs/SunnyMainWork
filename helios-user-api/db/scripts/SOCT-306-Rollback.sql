-- Drop columns from huser.consumer if they exist
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'huser'
          AND table_name = 'consumer'
          AND column_name = 'is_sso_user'
    ) THEN
        EXECUTE 'ALTER TABLE huser.consumer DROP COLUMN is_sso_user';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'huser'
          AND table_name = 'consumer'
          AND column_name = 'auth0_user_name'
    ) THEN
        EXECUTE 'ALTER TABLE huser.consumer DROP COLUMN auth0_user_name';
    END IF;
END
$$;

-- Drop column from huser.person if it exists
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'huser'
          AND table_name = 'person'
          AND column_name = 'person_unique_identifier'
    ) THEN
        EXECUTE 'ALTER TABLE huser.person DROP COLUMN person_unique_identifier';
    END IF;
END
$$;

-- First, ensure no NULLs exist before re-adding the NOT NULL constraint
UPDATE huser.person
SET email = 'test@xyz.com'
WHERE email IS NULL;

-- Then, reapply the NOT NULL constraint
ALTER TABLE huser.person
ALTER COLUMN email SET NOT NULL;