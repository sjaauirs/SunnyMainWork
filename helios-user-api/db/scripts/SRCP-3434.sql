-- Script to add new roles to the huser.role table, ensuring consistency across environments
DO $$
BEGIN
    -- Add the customer_admin role if it does not already exist
    IF NOT EXISTS (
        SELECT 1 
        FROM huser.role
        WHERE role_name = 'customer_admin' 
          AND role_code = 'rol-938b67672b3f4dcaba54435e95dda6c5'
    ) THEN
        INSERT INTO huser.role (role_name, role_code, role_description, create_ts, create_user, delete_nbr) 
        VALUES ('customer_admin', 'rol-938b67672b3f4dcaba54435e95dda6c5', 'Administrator at customer level', NOW(), 'SYSTEM', 0);
        RAISE NOTICE 'Role "customer_admin" added successfully.';
    ELSE
        RAISE NOTICE 'Role "customer_admin" already exists.';
    END IF;

    -- Add the sponsor_admin role if it does not already exist
    IF NOT EXISTS (
        SELECT 1 
        FROM huser.role
        WHERE role_name = 'sponsor_admin' 
          AND role_code = 'rol-2b394f0307554d99b106e8fe0518bd04'
    ) THEN
        INSERT INTO huser.role (role_name, role_code, role_description, create_ts, create_user, delete_nbr) 
        VALUES ('sponsor_admin', 'rol-2b394f0307554d99b106e8fe0518bd04', 'Administrator at sponsor level', NOW(), 'SYSTEM', 0);
        RAISE NOTICE 'Role "sponsor_admin" added successfully.';
    ELSE
        RAISE NOTICE 'Role "sponsor_admin" already exists.';
    END IF;

    -- Add the tenant_admin role if it does not already exist
    IF NOT EXISTS (
        SELECT 1 
        FROM huser.role
        WHERE role_name = 'tenant_admin' 
          AND role_code = 'rol-44e107f38dda4423bf653525fb9c1938'
    ) THEN
        INSERT INTO huser.role (role_name, role_code, role_description, create_ts, create_user, delete_nbr) 
        VALUES ('tenant_admin', 'rol-44e107f38dda4423bf653525fb9c1938', 'Administrator at tenant level', NOW(), 'SYSTEM', 0);
        RAISE NOTICE 'Role "tenant_admin" added successfully.';
    ELSE
        RAISE NOTICE 'Role "tenant_admin" already exists.';
    END IF;

    -- Add the report_user role if it does not already exist
    IF NOT EXISTS (
        SELECT 1 
        FROM huser.role
        WHERE role_name = 'report_user' 
          AND role_code = 'rol-f7ca3ef923614892907094230365bb82'
    ) THEN
        INSERT INTO huser.role (role_name, role_code, role_description, create_ts, create_user, delete_nbr) 
        VALUES ('report_user', 'rol-f7ca3ef923614892907094230365bb82', 'Report user', NOW(), 'SYSTEM', 0);
        RAISE NOTICE 'Role "report_user" added successfully.';
    ELSE
        RAISE NOTICE 'Role "report_user" already exists.';
    END IF;

END $$;

-- End of script


DO $$
BEGIN
    -- Drop the existing unique index if it exists
    IF EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'idx_person_id_role_id_delete_nbr') THEN
        DROP INDEX IF EXISTS huser.idx_person_id_role_id_delete_nbr;
        RAISE NOTICE 'Unique index "idx_person_id_role_id_delete_nbr" dropped successfully.';
    ELSE
        RAISE NOTICE 'Unique index "idx_person_id_role_id_delete_nbr" does not exist.';
    END IF;

    -- Add non-unique index on the same columns if it does not exist
    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'idx_person_id_role_id_delete_nbr_non_unique') THEN
        CREATE INDEX IF NOT EXISTS idx_person_id_role_id_delete_nbr_non_unique
            ON huser.person_role (person_id, role_id, delete_nbr);
        RAISE NOTICE 'Non-unique index "idx_person_id_role_id_delete_nbr_non_unique" created successfully.';
    ELSE
        RAISE NOTICE 'Non-unique index "idx_person_id_role_id_delete_nbr_non_unique" already exists.';
    END IF;

    -- Add new columns if they do not already exist
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'person_role' AND column_name = 'customer_code') THEN
        ALTER TABLE huser.person_role
            ADD COLUMN customer_code VARCHAR(50) NULL;
        RAISE NOTICE 'Column "customer_code" added successfully.';
    ELSE
        RAISE NOTICE 'Column "customer_code" already exists.';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'person_role' AND column_name = 'sponsor_code') THEN
        ALTER TABLE huser.person_role
            ADD COLUMN sponsor_code VARCHAR(50) NULL;
        RAISE NOTICE 'Column "sponsor_code" added successfully.';
    ELSE
        RAISE NOTICE 'Column "sponsor_code" already exists.';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'person_role' AND column_name = 'tenant_code') THEN
        ALTER TABLE huser.person_role
            ADD COLUMN tenant_code VARCHAR(50) NULL;
        RAISE NOTICE 'Column "tenant_code" added successfully.';
    ELSE
        RAISE NOTICE 'Column "tenant_code" already exists.';
    END IF;

    -- Add new unique index if it does not already exist
    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'idx_person_role_2') THEN
        CREATE UNIQUE INDEX IF NOT EXISTS idx_person_role_2
            ON huser.person_role (person_id, role_id, customer_code, sponsor_code, tenant_code, delete_nbr);
        RAISE NOTICE 'Unique index "idx_person_role_2" created successfully.';
    ELSE
        RAISE NOTICE 'Unique index "idx_person_role_2" already exists.';
    END IF;

END $$;

-- End of script
