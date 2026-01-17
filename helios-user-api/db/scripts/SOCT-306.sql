-- Add columns to huser.consumer if they do not exist
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'huser'
          AND table_name = 'consumer'
          AND column_name = 'is_sso_user'
    ) THEN
        EXECUTE 'ALTER TABLE huser.consumer ADD COLUMN is_sso_user BOOLEAN DEFAULT FALSE';
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'huser'
          AND table_name = 'consumer'
          AND column_name = 'auth0_user_name'
    ) THEN
        EXECUTE 'ALTER TABLE huser.consumer ADD COLUMN auth0_user_name VARCHAR(255)';
    END IF;
END
$$;

-- Add column to huser.person if it does not exist
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'huser'
          AND table_name = 'person'
          AND column_name = 'person_unique_identifier'
    ) THEN
        EXECUTE 'ALTER TABLE huser.person ADD COLUMN person_unique_identifier VARCHAR(255)';
    END IF;
END
$$;

--Script to make the email column nullable in the huser.person 
ALTER TABLE huser.person
ALTER COLUMN email DROP NOT NULL;

-- Update person_unique_identifier with email for existing users
UPDATE huser.person
SET person_unique_identifier = email,
    update_ts = CURRENT_TIMESTAMP AT TIME ZONE 'UTC'
WHERE delete_nbr = 0;

