-- ==================================================================================================
-- 🚨 Rollback Script : Soft delete trivia setup (PK-based delete_nbr)
-- 📌 Purpose         : Soft delete trivia, trivia_question_group, and trivia_question 
--                      created or updated for a specific tenant and trivia setup.
-- 🧑 Author          : Siva Krishna
-- 📅 Date            : 2025-10-16
-- 🧾 Jira            : RES-684 (Defect)
-- ⚠️ Inputs          : HAP_TENANT_CODE (replace placeholder below)
-- 📤 Output    : Soft delete trivia, trivia_question and trivia_question_group
-- 🔗 Script URL: NA
-- 📝 Notes     : NA
-- ==================================================================================================

DO
$$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>';  -- Replace with actual HAP Tenant Code
    v_task_external_code TEXT := 'lear_abou_pres_home_deli'; 
    v_cta_task_external_code TEXT := 'play_now';
    v_task_reward_id BIGINT;
    v_trivia_id BIGINT;
BEGIN
    -- Fetch task_reward_id for the given tenant and triviaTaskExternalCode
    SELECT task_reward_id INTO v_task_reward_id
    FROM task.task_reward
    WHERE task_external_code = v_task_external_code
      AND tenant_code = v_tenant_code
      AND delete_nbr = 0
    LIMIT 1;

    IF v_task_reward_id IS NULL THEN
        RAISE NOTICE '⚠️ No task_reward found for tenant: % and task_external_code: %', v_tenant_code, v_task_external_code;
        RETURN;
    END IF;

    -- Fetch trivia_id
    SELECT trivia_id INTO v_trivia_id
    FROM task.trivia
    WHERE task_reward_id = v_task_reward_id
      AND cta_task_external_code = v_cta_task_external_code
      AND delete_nbr = 0
    LIMIT 1;

    IF v_trivia_id IS NULL THEN
        RAISE NOTICE '⚠️ No trivia found for tenant: % (task_reward_id: %)', v_tenant_code, v_task_reward_id;
        RETURN;
    END IF;

    RAISE NOTICE '🧩 Rolling back trivia setup for tenant: %, trivia_id: %', v_tenant_code, v_trivia_id;

    -- 1️ Soft delete trivia_question_group entries (set delete_nbr = PK)
    UPDATE task.trivia_question_group
    SET delete_nbr = trivia_question_group_id,
        update_ts = now(),
        update_user = 'ROLLBACK'
    WHERE trivia_id = v_trivia_id
      AND delete_nbr = 0;

    RAISE NOTICE '✅ trivia_question_group entries soft deleted (delete_nbr = PK) for trivia_id: %', v_trivia_id;

    -- 2️ Soft delete all trivia_question entries linked to the trivia (set delete_nbr = PK)
    UPDATE task.trivia_question
    SET delete_nbr = trivia_question_id,
        update_ts = now(),
        update_user = 'ROLLBACK'
    WHERE trivia_question_id IN (
        SELECT trivia_question_id
        FROM task.trivia_question_group
        WHERE trivia_id = v_trivia_id
    )
    AND delete_nbr = 0;

    RAISE NOTICE '✅ trivia_question entries soft deleted (delete_nbr = PK) for linked questions of trivia_id: %', v_trivia_id;

    -- 3️ Soft delete trivia entry itself (set delete_nbr = PK)
    UPDATE task.trivia
    SET delete_nbr = trivia_id,
        update_ts = now(),
        update_user = 'ROLLBACK'
    WHERE trivia_id = v_trivia_id
      AND delete_nbr = 0;

    RAISE NOTICE '✅ trivia entry soft deleted (delete_nbr = PK, trivia_id: %)', v_trivia_id;

    RAISE NOTICE '🎯 ✅ Rollback completed successfully for tenant: %', v_tenant_code;
END
$$;
