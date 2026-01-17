-- ============================================================================
-- 🚀 Script    : Update requiredUnits for task_completion_criteria_json
-- 📌 Purpose   : Updates the "requiredUnits" value from 24 → 20
--                for a specific task_external_code and tenant_code.
-- 🧑 Author    : Ankita Sridhar
-- 📅 Date      : 10/13/2025
-- 🧾 Jira      : RES-756
-- ⚠️ Inputs    : v_tenant_code, v_task_external_code
-- 📤 Output    : Updated task.task_reward.task_completion_criteria_json
-- ============================================================================

DO $$
DECLARE
    v_tenant_code        TEXT := '<KP-TENANT CODE>';  -- replace as needed
    v_task_external_code TEXT := 'get_your_z_s_2026';                     -- replace as needed
BEGIN
    UPDATE task.task_reward
    SET task_completion_criteria_json = jsonb_set(
        task_completion_criteria_json::jsonb,
        '{healthCriteria,requiredUnits}',
        '20'::jsonb,
        false
    )
    WHERE tenant_code = v_tenant_code
      AND task_external_code = v_task_external_code
      AND delete_nbr = 0;

    RAISE NOTICE '✅ Updated requiredUnits to 20 for tenant: %, task: %',
        v_tenant_code, v_task_external_code;
END $$;
