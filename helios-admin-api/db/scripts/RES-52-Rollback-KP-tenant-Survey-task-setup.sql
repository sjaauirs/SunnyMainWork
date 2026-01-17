-- =================================================================================================================================
-- 🚨 Rollback Script : RES-52 "Your Voice Matters" For KP Tenant Inserts Only Rollback
-- 📌 Purpose         : Rollback inserted data created by RES-52 for KP-tenant-code
-- 🧑 Author          : Siva Krishna
-- 📅 Date            : 2025-10-08
-- ⚠️ Inputs          : KP_TENANT_CODE
-- 📝 Notes           : Soft delete by setting delete_nbr = primary key id. Updates are not rolled back here.
-- =================================================================================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<KP_TENANT_CODE>'; -- KP tenant only
    
    v_task_external_code TEXT := 'your_voic_matt';
    v_task_reward_code TEXT;
    v_task_id BIGINT;
    v_cohort_id BIGINT;
    v_script_code TEXT := 'src-4fdd3ae6573b44eda0d343a775a3350c';
BEGIN
    -- Find task_reward_code + task_id
    SELECT task_reward_code, task_id
    INTO v_task_reward_code, v_task_id
    FROM task.task_reward
    WHERE task_external_code = v_task_external_code
      AND tenant_code = v_tenant_code
      AND delete_nbr = 0
    LIMIT 1;

    -- Rollback cohort_tenant_task_reward
    UPDATE cohort.cohort_tenant_task_reward
    SET delete_nbr = cohort_tenant_task_reward_id,
        update_ts = NOW(),
        update_user = 'ROLLBACK_SCRIPT'
    WHERE tenant_code = v_tenant_code
      AND task_reward_code = v_task_reward_code
      AND delete_nbr = 0;

    -- Rollback task_reward
    UPDATE task.task_reward
    SET delete_nbr = task_reward_id,
        update_ts = NOW(),
        update_user = 'ROLLBACK_SCRIPT'
    WHERE task_reward_code = v_task_reward_code
      AND tenant_code = v_tenant_code
      AND delete_nbr = 0;

    -- Rollback task_detail
    UPDATE task.task_detail
    SET delete_nbr = task_detail_id,
        update_ts = NOW(),
        update_user = 'ROLLBACK_SCRIPT'
    WHERE task_id = v_task_id
      AND tenant_code = v_tenant_code
      AND delete_nbr = 0;

    -- Rollback task
    UPDATE task.task
    SET delete_nbr = task_id,
        update_ts = NOW(),
        update_user = 'ROLLBACK_SCRIPT'
    WHERE task_id = v_task_id
      AND delete_nbr = 0;

    -- Rollback cohort (Survey)
    SELECT cohort_id INTO v_cohort_id
    FROM cohort.cohort
    WHERE cohort_name = 'Survey'
      AND delete_nbr = 0
    LIMIT 1;

    IF v_cohort_id IS NOT NULL THEN
        UPDATE cohort.cohort
        SET delete_nbr = cohort_id,
            update_ts = NOW(),
            update_user = 'ROLLBACK_SCRIPT'
        WHERE cohort_id = v_cohort_id
          AND delete_nbr = 0;
    END IF;

    -- Rollback admin.script
    UPDATE admin.script
    SET delete_nbr = script_id,
        update_ts = NOW(),
        update_user = 'ROLLBACK_SCRIPT'
    WHERE script_code = v_script_code
      AND delete_nbr = 0;

    -- Rollback admin.tenant_task_reward_script
    UPDATE admin.tenant_task_reward_script
    SET delete_nbr = tenant_task_reward_script_id,
        update_ts = NOW(),
        update_user = 'ROLLBACK_SCRIPT'
    WHERE tenant_code = v_tenant_code
      AND script_id IN (SELECT script_id FROM admin.script WHERE script_code = v_script_code)
      AND task_reward_code = v_task_reward_code
      AND delete_nbr = 0;

    RAISE NOTICE '[Rollback Completed] All inserts for RES-51 rolled back for tenant_code=%', v_tenant_code;
END $$;