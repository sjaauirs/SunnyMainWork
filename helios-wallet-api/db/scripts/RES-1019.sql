-- ============================================================================
-- 🚀 Script    : Modify wallet_transaction_code Column in your_table_name
-- 📌 Purpose   : Remove the length limit (50) on the 'wallet_transaction_code' column
--                in the 'your_table_name' table, making it a character varying 
--                without a length restriction.
-- 🧑 Author    : Kumar sirikonda
-- 📅 Date      : 2025-11-06
-- 🧾 Jira      : RES-1019
-- ⚠️ Inputs    : None
-- 📤 Output    : Modifies the 'wallet_transaction_code' column to remove its length limit.
-- 🔗 Script URL: <Link-to-your-script-url>
-- 📝 Notes     : The script is idempotent — safe to execute multiple times as it
--                checks for the column existence before attempting to alter it.
-- ============================================================================

DO
$$
BEGIN
    -- Check if column 'wallet_transaction_code' exists and is of type character varying(50)
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'task'
          AND table_name = 'consumer_task'
          AND column_name = 'wallet_transaction_code'
          AND data_type = 'character varying'
          AND character_maximum_length = 50
    ) THEN
        -- Alter the column to remove the length limit
        ALTER TABLE task.consumer_task
        ALTER COLUMN wallet_transaction_code TYPE character varying;
        
        RAISE NOTICE 'Column "wallet_transaction_code" updated to remove length limit in table "task.consumer_task".';
    ELSE
        RAISE NOTICE 'Column "wallet_transaction_code" does not exist or is already modified in table "task.consumer_task".';
    END IF;
END
$$;