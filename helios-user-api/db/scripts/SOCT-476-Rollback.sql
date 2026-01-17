-- 1️⃣ Drop the new index on member_id
DROP INDEX IF EXISTS huser.idx_consumer_1;

-- 2️⃣ If column member_id exists, drop NOT NULL constraint first (in case it was applied)
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'huser'
          AND table_name = 'consumer'
          AND column_name = 'member_id'
    ) THEN
        BEGIN
            ALTER TABLE huser.consumer
            ALTER COLUMN member_id DROP NOT NULL;
        EXCEPTION
            WHEN undefined_column THEN
                -- Column not found or already nullable, ignore
                NULL;
        END;
    END IF;
END
$$;

-- 3️⃣ Drop member_id column entirely
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'huser'
          AND table_name = 'consumer'
          AND column_name = 'member_id'
    ) THEN
        ALTER TABLE huser.consumer
        DROP COLUMN member_id;
    END IF;
END
$$;

-- 4️⃣ Recreate the original index using mem_nbr
CREATE UNIQUE INDEX IF NOT EXISTS idx_consumer_1
ON huser.consumer USING btree
(
    tenant_code COLLATE pg_catalog."default" ASC NULLS LAST,
    mem_nbr COLLATE pg_catalog."default" ASC NULLS LAST,
    delete_nbr ASC NULLS LAST
)
TABLESPACE pg_default;

-- 5️⃣ Optionally, restore any previous redundant index if needed
CREATE UNIQUE INDEX IF NOT EXISTS idx_tenant_code_mem_nbr_delete_nbr
ON huser.consumer USING btree
(
    tenant_code COLLATE pg_catalog."default" ASC NULLS LAST,
    mem_nbr COLLATE pg_catalog."default" ASC NULLS LAST,
    delete_nbr ASC NULLS LAST
)
TABLESPACE pg_default;
