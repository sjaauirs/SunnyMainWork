-- ============================================================================
-- ⏪ Script    : Rollback requiredUnits change in task_completion_criteria_json
-- 📌 Purpose   : Reverts "requiredUnits" from 20 → 24
-- 🧑 Author    : Ankita Sridhar
-- 📅 Date      : 10/13/2025
-- 🧾 Jira      : RES-756
-- ⚠️ Inputs    : v_tenant_code, v_task_external_code
-- 📤 Output    : task.task_reward.task_completion_criteria_json restored
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
        '24'::jsonb,
        false
    )
    WHERE tenant_code = v_tenant_code
      AND task_external_code = v_task_external_code
      AND delete_nbr = 0
      -- Guard so we only roll back rows that currently have 20
      AND (task_completion_criteria_json::jsonb -> 'healthCriteria' ->> 'requiredUnits')::INT = 20;

    RAISE NOTICE '⏪ Rolled back requiredUnits to 24 for tenant: %, task: %',
        v_tenant_code, v_task_external_code;
END $$;
