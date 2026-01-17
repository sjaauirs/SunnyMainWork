-- ============================================================================
-- 📌 Purpose   : 
--   - Rollback: Drop the step_config column from tenant.flow_step
-- 🧑 Author    : Saurabh
-- 📅 Date      : 2025-09-25
-- 🧾 Jira      : BEN-8
-- ⚠️ Inputs    : None
-- 📤 Output    : 
--   - Removes `step_config` JSONB column if it exists
-- 📝 Notes     : 
--   - Uses IF EXISTS check for idempotency
--   - Safe to run multiple times (skips if column doesn’t exist)
-- ============================================================================

DO $$
BEGIN
    -- Check if column exists
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns 
        WHERE table_schema = 'tenant'
          AND table_name   = 'flow_step'
          AND column_name  = 'step_config'
    ) THEN
        -- Drop the column
        ALTER TABLE tenant.flow_step
        DROP COLUMN step_config;

        RAISE NOTICE '♻️ Column step_config dropped from tenant.flow_step';
    ELSE
        RAISE NOTICE 'ℹ️ Column step_config does not exist in tenant.flow_step, skipping';
    END IF;
END $$;
