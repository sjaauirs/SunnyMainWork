DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns 
        WHERE table_schema = 'huser'
          AND table_name = 'consumer'
          AND column_name = 'member_id'
    ) THEN
        ALTER TABLE huser.consumer
        ADD COLUMN member_id varchar(225) NULL;
     END IF; 
END
$$;

DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns 
        WHERE table_schema = 'huser'
          AND table_name = 'consumer'
          AND column_name = 'member_id'
    ) THEN
       UPDATE huser.consumer
    SET member_id = mem_nbr
    WHERE mem_nbr IS NOT NULL AND TRIM(mem_nbr) != ''
    AND (member_id IS NULL OR TRIM(member_id) = '');  
     END IF; 
END
$$;
-- 1️ Drop redundant index if exists
DROP INDEX IF EXISTS huser.idx_tenant_code_mem_nbr_delete_nbr;
 
-- 2️ Drop old index using mem_nbr
DROP INDEX IF EXISTS huser.idx_consumer_1;
 
-- 3️ Create new index with member_id
CREATE UNIQUE INDEX IF NOT EXISTS idx_consumer_1
ON huser.consumer USING btree
(
    tenant_code COLLATE pg_catalog."default" ASC NULLS LAST,
    member_id COLLATE pg_catalog."default" ASC NULLS LAST,
    delete_nbr ASC NULLS LAST
)
TABLESPACE pg_default;
 
-- 4️ Check for any NULL member_id before altering column
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 
        FROM huser.consumer 
        WHERE member_id IS NULL
    ) THEN
        RAISE EXCEPTION 'Cannot set member_id to NOT NULL: NULL values exist';
    ELSE
        -- 5️ Alter column to set NOT NULL
        ALTER TABLE huser.consumer
        ALTER COLUMN member_id SET NOT NULL;
    END IF;
END
$$;

 
