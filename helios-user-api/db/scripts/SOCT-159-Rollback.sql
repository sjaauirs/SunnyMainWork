-- Rollback: Remove middle_name from huser.person
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'huser'
          AND table_name = 'person'
          AND column_name = 'middle_name'
    ) THEN
        ALTER TABLE huser.person
        DROP COLUMN middle_name;
    END IF;
END $$;

-- Rollback: Remove new columns from huser.consumer
DO $$
BEGIN
    -- region_code
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'huser'
          AND table_name = 'consumer'
          AND column_name = 'region_code'
    ) THEN
        ALTER TABLE huser.consumer
        DROP COLUMN region_code;
    END IF;

    -- subscriber_mem_nbr_prefix
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'huser'
          AND table_name = 'consumer'
          AND column_name = 'subscriber_mem_nbr_prefix'
    ) THEN
        ALTER TABLE huser.consumer
        DROP COLUMN subscriber_mem_nbr_prefix;
    END IF;

    -- mem_nbr_prefix
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'huser'
          AND table_name = 'consumer'
          AND column_name = 'mem_nbr_prefix'
    ) THEN
        ALTER TABLE huser.consumer
        DROP COLUMN mem_nbr_prefix;
    END IF;

    -- plan_id
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'huser'
          AND table_name = 'consumer'
          AND column_name = 'plan_id'
    ) THEN
        ALTER TABLE huser.consumer
        DROP COLUMN plan_id;
    END IF;

    -- subgroup_id
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'huser'
          AND table_name = 'consumer'
          AND column_name = 'subgroup_id'
    ) THEN
        ALTER TABLE huser.consumer
        DROP COLUMN subgroup_id;
    END IF;

    -- plan_type
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'huser'
          AND table_name = 'consumer'
          AND column_name = 'plan_type'
    ) THEN
        ALTER TABLE huser.consumer
        DROP COLUMN plan_type;
    END IF;
END $$;

-- Rollback: Delete address_type_name 'HOME' from address_type table
DO $$
BEGIN
    DELETE FROM huser.address_type
    WHERE LOWER(address_type_name) = 'home'
      AND delete_nbr = 0
      AND create_user = 'system';
END $$;