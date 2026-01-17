-- =============================================================================
-- 🚀 Script    : Rollback ES Label in task_completion_criteria_json
-- 📌 Purpose   : Rollback the 'es' label under healthCriteria.unitLabel in task_completion_criteria_json
-- 🧑 Author    : Siva Krishna Reddy
-- 📅 Date      : 28-Oct-2025
-- 🧾 Jira      : RES-963
-- ⚙️ Inputs    : KP-TENANT-CODE
--      - TENANT_CODE
--      - TASK_EXTERNAL_CODE
--      - ES_LABEL
-- =============================================================================

DO
$$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>';   -- 🔸 Input: Tenant Code
    
    v_task_external_code TEXT := 'stre_your_body_2026';  
    v_es_label TEXT := 'sessions'; 
BEGIN
    UPDATE task.task_reward
    SET task_completion_criteria_json = jsonb_set(
        task_completion_criteria_json,
        '{healthCriteria,unitLabel,es}',
        to_jsonb(v_es_label::text),
        TRUE
    )
    WHERE tenant_code = v_tenant_code
      AND task_external_code = v_task_external_code
	  AND delete_nbr = 0;

    RAISE NOTICE '✅ Rollbacked ES label to "%" for task_external_code: %, tenant_code: %',
        v_es_label, v_task_external_code, v_tenant_code;
END
$$;
