-- Script to remove specific roles from the huser.role table

DO $$
BEGIN
    -- Remove the customer_admin role if it exists
    IF EXISTS (
        SELECT 1 
        FROM huser.role
        WHERE role_name = 'customer_admin' 
          AND role_code = 'rol-938b67672b3f4dcaba54435e95dda6c5'
    ) THEN
        DELETE FROM huser.role
        WHERE role_name = 'customer_admin' 
          AND role_code = 'rol-938b67672b3f4dcaba54435e95dda6c5';
        RAISE NOTICE 'Role "customer_admin" removed successfully.';
    ELSE
        RAISE NOTICE 'Role "customer_admin" does not exist.';
    END IF;

    -- Remove the sponsor_admin role if it exists
    IF EXISTS (
        SELECT 1 
        FROM huser.role
        WHERE role_name = 'sponsor_admin' 
          AND role_code = 'rol-2b394f0307554d99b106e8fe0518bd04'
    ) THEN
        DELETE FROM huser.role
        WHERE role_name = 'sponsor_admin' 
          AND role_code = 'rol-2b394f0307554d99b106e8fe0518bd04';
        RAISE NOTICE 'Role "sponsor_admin" removed successfully.';
    ELSE
        RAISE NOTICE 'Role "sponsor_admin" does not exist.';
    END IF;

    -- Remove the tenant_admin role if it exists
    IF EXISTS (
        SELECT 1 
        FROM huser.role
        WHERE role_name = 'tenant_admin' 
          AND role_code = 'rol-44e107f38dda4423bf653525fb9c1938'
    ) THEN
        DELETE FROM huser.role
        WHERE role_name = 'tenant_admin' 
          AND role_code = 'rol-44e107f38dda4423bf653525fb9c1938';
        RAISE NOTICE 'Role "tenant_admin" removed successfully.';
    ELSE
        RAISE NOTICE 'Role "tenant_admin" does not exist.';
    END IF;

    -- Remove the report_user role if it exists
    IF EXISTS (
        SELECT 1 
        FROM huser.role
        WHERE role_name = 'report_user' 
          AND role_code = 'rol-f7ca3ef923614892907094230365bb82'
    ) THEN
        DELETE FROM huser.role
        WHERE role_name = 'report_user' 
          AND role_code = 'rol-f7ca3ef923614892907094230365bb82';
        RAISE NOTICE 'Role "report_user" removed successfully.';
    ELSE
        RAISE NOTICE 'Role "report_user" does not exist.';
    END IF;

END $$;

-- End of script


DO $$
BEGIN
    -- Drop the unique index idx_person_role_2 if it exists
    IF EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'idx_person_role_2') THEN
        DROP INDEX IF EXISTS idx_person_role_2;
        RAISE NOTICE 'Unique index "idx_person_role_2" dropped successfully.';
    ELSE
        RAISE NOTICE 'Unique index "idx_person_role_2" does not exist.';
    END IF;

    -- Drop the non-unique index idx_person_id_role_id_delete_nbr_non_unique if it exists
    IF EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'idx_person_id_role_id_delete_nbr_non_unique') THEN
        DROP INDEX IF EXISTS idx_person_id_role_id_delete_nbr_non_unique;
        RAISE NOTICE 'Non-unique index "idx_person_id_role_id_delete_nbr_non_unique" dropped successfully.';
    ELSE
        RAISE NOTICE 'Non-unique index "idx_person_id_role_id_delete_nbr_non_unique" does not exist.';
    END IF;

    -- Drop the columns if they exist
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'person_role' AND column_name = 'customer_code') THEN
        ALTER TABLE huser.person_role
            DROP COLUMN IF EXISTS customer_code;
        RAISE NOTICE 'Column "customer_code" dropped successfully.';
    ELSE
        RAISE NOTICE 'Column "customer_code" does not exist.';
    END IF;

    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'person_role' AND column_name = 'sponsor_code') THEN
        ALTER TABLE huser.person_role
            DROP COLUMN IF EXISTS sponsor_code;
        RAISE NOTICE 'Column "sponsor_code" dropped successfully.';
    ELSE
        RAISE NOTICE 'Column "sponsor_code" does not exist.';
    END IF;

    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'person_role' AND column_name = 'tenant_code') THEN
        ALTER TABLE huser.person_role
            DROP COLUMN IF EXISTS tenant_code;
        RAISE NOTICE 'Column "tenant_code" dropped successfully.';
    ELSE
        RAISE NOTICE 'Column "tenant_code" does not exist.';
    END IF;

    -- Recreate the original unique index idx_person_id_role_id_delete_nbr if it was dropped
    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'idx_person_id_role_id_delete_nbr') THEN
        CREATE UNIQUE INDEX IF NOT EXISTS idx_person_id_role_id_delete_nbr
            ON huser.person_role (person_id, role_id, delete_nbr);
        RAISE NOTICE 'Unique index "idx_person_id_role_id_delete_nbr" recreated successfully.';
    ELSE
        RAISE NOTICE 'Unique index "idx_person_id_role_id_delete_nbr" already exists.';
    END IF;

END $$;

-- End of script