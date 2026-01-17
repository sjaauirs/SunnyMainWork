-- ============================================================================
-- 🚀 Script    : Rollback - Remove subscription_status Column from huser.consumer
-- 📌 Purpose   : Drop the 'subscription_status' JSONB column from the
--                huser.consumer table if it exists.
-- 🧑 Author    : Siva Krishna Reddy
-- 📅 Date      : 2025-10-23
-- 🧾 Jira      : RES-725
-- ⚠️ Inputs    : None
-- 📤 Output    : Removes 'subscription_status' column from huser.consumer table
-- 🔗 Script URL: N/A
-- 📝 Notes     : 
--   - Idempotent: Safe to execute multiple times.
--   - Checks for column existence before attempting to drop.
-- ============================================================================

-- ============================================================================
-- 🚀 Script    : Rollback - Remove subscription_status_json Column from huser.consumer
-- 📌 Purpose   : Drop the 'subscription_status_json' JSONB column from the
--                huser.consumer table if it exists.
-- 🧑 Author    : Siva Krishna Reddy
-- 📅 Date      : 2025-10-23
-- 🧾 Jira      : RES-725
-- ⚠️ Inputs    : None
-- 📤 Output    : Removes 'subscription_status_json' column from huser.consumer table
-- 🔗 Script URL: N/A
-- 📝 Notes     :
--   - Idempotent: Safe to execute multiple times.
--   - Includes logging using RAISE NOTICE for clarity.
-- ============================================================================

DO
$$
DECLARE
    v_column_exists BOOLEAN;
BEGIN
    -- Check if column exists
    SELECT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'huser'
          AND table_name = 'consumer'
          AND column_name = 'subscription_status_json'
    )
    INTO v_column_exists;

    -- Drop the column if it exists
    IF v_column_exists THEN
        RAISE NOTICE '🔍 Column "subscription_status_json" found in table "huser.consumer". Proceeding to drop.';
        
        ALTER TABLE huser.consumer
        DROP COLUMN subscription_status_json;
        
        RAISE NOTICE '✅ Column "subscription_status_json" successfully dropped from table "huser.consumer".';
    ELSE
        RAISE NOTICE 'ℹ️ Column "subscription_status_json" does not exist in table "huser.consumer". No action taken.';
    END IF;
END
$$;

