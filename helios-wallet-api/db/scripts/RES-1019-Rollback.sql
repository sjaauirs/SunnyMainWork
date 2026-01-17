-- ============================================================================
-- 🔄 Rollback Script: Revert 'wallet_transaction_code' Column to character varying(50)
-- 📌 Purpose   : Revert the 'wallet_transaction_code' column in the 'task.consumer_task'
--                table to its original definition of character varying(50).
-- 🧑 Author    : Kumar sirikonda
-- 📅 Date      : 2025-11-06
-- 🧾 Jira      : RES-1019
-- ⚠️ Inputs    : None
-- 📤 Output    : Reverts 'wallet_transaction_code' column to 'character varying(50)'.
-- 🔗 Script URL: <Link-to-your-script-url>
-- 📝 Notes     : The script is idempotent — safe to execute multiple times as it
--                checks for the column existence before attempting to alter it.
-- ============================================================================

DO
$$
BEGIN
    -- Check if column 'wallet_transaction_code' exists and is of type character varying
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'task'
          AND table_name = 'consumer_task'
          AND column_name = 'wallet_transaction_code'
          AND data_type = 'character varying'
    ) THEN
        -- Rollback: Revert the column to character varying(50)
        ALTER TABLE task.consumer_task
        ALTER COLUMN wallet_transaction_code TYPE character varying(50) COLLATE pg_catalog."default";
        
        RAISE NOTICE 'Column "wallet_transaction_code" reverted to character varying(50) in table "task.consumer_task".';
    ELSE
        RAISE NOTICE 'Column "wallet_transaction_code" does not exist in table "task.consumer_task".';
    END IF;
END
$$;
