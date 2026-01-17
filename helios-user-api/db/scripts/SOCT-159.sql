-- Add new column middle_name to huser.person
DO $$
BEGIN
    -- Check if column doesn't already exist
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'huser'
          AND table_name = 'person'
          AND column_name = 'middle_name'
    ) THEN
        -- Add the column
        ALTER TABLE huser.person
        ADD COLUMN middle_name varchar(80);
    END IF;
END $$;

-- Add new columns to huser.consumer
DO $$
BEGIN
    -- region_code
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'huser'
          AND table_name = 'consumer'
          AND column_name = 'region_code'
    ) THEN
        ALTER TABLE huser.consumer
        ADD COLUMN region_code varchar(80);
    END IF;

    -- subscriber_mem_nbr_prefix
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'huser'
          AND table_name = 'consumer'
          AND column_name = 'subscriber_mem_nbr_prefix'
    ) THEN
        ALTER TABLE huser.consumer
        ADD COLUMN subscriber_mem_nbr_prefix varchar(80);
    END IF;

    -- mem_nbr_prefix
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'huser'
          AND table_name = 'consumer'
          AND column_name = 'mem_nbr_prefix'
    ) THEN
        ALTER TABLE huser.consumer
        ADD COLUMN mem_nbr_prefix varchar(80);
    END IF;

    -- plan_id
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'huser'
          AND table_name = 'consumer'
          AND column_name = 'plan_id'
    ) THEN
        ALTER TABLE huser.consumer
        ADD COLUMN plan_id varchar(80);
    END IF;

    -- subgroup_id
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'huser'
          AND table_name = 'consumer'
          AND column_name = 'subgroup_id'
    ) THEN
        ALTER TABLE huser.consumer
        ADD COLUMN subgroup_id varchar(80);
    END IF;

    -- plan_type
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'huser'
          AND table_name = 'consumer'
          AND column_name = 'plan_type'
    ) THEN
        ALTER TABLE huser.consumer
        ADD COLUMN plan_type varchar(80);
    END IF;
END $$;

-- Insert address_type_name 'HOME' into address_type table
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM huser.address_type
        WHERE LOWER(address_type_name) = 'home'
          AND delete_nbr = 0
    ) THEN
        INSERT INTO huser.address_type (
            address_type_id,
            address_type_code,
            address_type_name,
            description,
            create_ts,
            update_ts,
            create_user,
            update_user,
            delete_nbr
        )
        VALUES (
            2,
            'atc-' || gen_random_uuid(),
            'HOME',
            'Home address',
            CURRENT_TIMESTAMP,
            CURRENT_TIMESTAMP,
            'system',
            'system',
            0
        );
    END IF;
END $$;