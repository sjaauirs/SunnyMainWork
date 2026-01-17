-- ============================================================================
-- 🚀 Script    : Add subscription_status Column to huser.consumer
-- 📌 Purpose   : Add a new column 'subscription_status' of type JSONB to the
--                huser.consumer table if it does not already exist.
-- 🧑 Author    : Siva Krishna Reddy
-- 📅 Date      : 2025-10-23
-- 🧾 Jira      : RES-725
-- ⚠️ Inputs    : None
-- 📤 Output    : Adds 'subscription_status' JSONB column to huser.consumer table
-- 🔗 Script URL: N/A
-- 📝 Notes     : The script is idempotent — safe to execute multiple times as it
--                checks for the column existence before attempting to add it.
-- ============================================================================


DO
$$
BEGIN
    -- Check if column does not exist
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'huser'
          AND table_name = 'consumer'
          AND column_name = 'subscription_status_json'
    ) THEN
        -- Add the new column
        ALTER TABLE huser.consumer
        ADD COLUMN subscription_status_json JSONB;
        
        RAISE NOTICE 'Column "subscription_status_json" added to table "huser.consumer".';
    ELSE
        RAISE NOTICE 'Column "subscription_status_json" already exists in table "huser.consumer".';
    END IF;
END
$$;

