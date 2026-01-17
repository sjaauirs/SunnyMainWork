-- 🧑 Author    : Saurabh
-- 📅 Date      : 2025-09-25
-- 🧾 Jira      : BEN-8
-- ⚠️ Inputs    : None
-- 📤 Output    : 
--   - Adds `step_config` JSONB column if it doesn’t already exist
-- 📝 Notes     : 
--   - Column is nullable by default
--   - Safe to run multiple times (no error if column already exists)
-- ============================================================================

DO $$
BEGIN
    -- Check if column exists
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_schema = 'tenant'
          AND table_name   = 'flow_step'
          AND column_name  = 'step_config'
    ) THEN
        -- Add new column
        ALTER TABLE tenant.flow_step
        ADD COLUMN step_config JSONB NULL;
        
        RAISE NOTICE '✅ Column step_config added to tenant.flow_step';
    ELSE
        RAISE NOTICE 'ℹ️ Column step_config already exists in tenant.flow_step, skipping';
    END IF;
END $$;
