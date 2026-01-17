-- ============================================================================
-- 🚀 Script    : Soft deletes task rewards & cohort task links  
-- 📌 Purpose   : Rollback cohort_tenant_task_reward and task_reward for sweepstakes_winners task
-- 🧑 Author    : Kumaraswamy / Siva Krishna 
-- 📅 Date      : 04-12-2025
-- 🧾 Jira      : RES-928
-- ⚠️ Inputs    : Array of Navitus 2026 Tenants
-- 📤 Output    : Soft deletes task rewards & cohort task links  
-- 🔗 Script URL: NA
-- 📝 Notes     : Execute the script only for NAVITUS 2026 Tenants and execute only if Forward script executes.
-- ============================================================================

DO $$
DECLARE
    -- Inputs
    v_tenant_codes TEXT[] := ARRAY[
        '<NAVITUS-2026-TENANT>',
        '<NAVITUS-2026-TENANT>'
    ];

    v_task_external_code TEXT := 'swee_winn';
    v_cohort_name TEXT := 'sweepstakes_winner';

    -- Loop variable
    v_tenant TEXT;

    -- Records to cleanup
    v_task_reward_code TEXT;
    v_cohort_id BIGINT;

BEGIN
    -- Fetch cohort_id once
    SELECT cohort_id INTO v_cohort_id
    FROM cohort.cohort
    WHERE cohort_name = v_cohort_name AND delete_nbr = 0
    LIMIT 1;

    IF v_cohort_id IS NULL THEN
        RAISE NOTICE '⚠️ Cohort "%" not found. Nothing to rollback.', v_cohort_name;
        RETURN;
    ELSE
        RAISE NOTICE 'Cohort found: cohort_id = %', v_cohort_id;
    END IF;


    -- ======================================================
    -- Loop through tenants
    -- ======================================================
    FOREACH v_tenant IN ARRAY v_tenant_codes LOOP
        RAISE NOTICE 'Processing rollback for tenant: %', v_tenant;

        -- Find the task_reward_code to delete
        SELECT task_reward_code INTO v_task_reward_code
        FROM task.task_reward
        WHERE tenant_code = v_tenant
          AND task_external_code = v_task_external_code
          AND delete_nbr = 0
        LIMIT 1;

        IF v_task_reward_code IS NULL THEN
            RAISE NOTICE '❗ No active task_reward found for tenant %, skipping.', v_tenant;
            CONTINUE;
        ELSE
            RAISE NOTICE 'Found task_reward_code = % for tenant %', v_task_reward_code, v_tenant;
        END IF;


        -- ======================================================
        -- Soft Delete: cohort.cohort_tenant_task_reward
        -- ======================================================
        UPDATE cohort.cohort_tenant_task_reward
        SET delete_nbr = 1,
            update_ts = NOW(),
            update_user = 'SYSTEM-ROLLBACK'
        WHERE cohort_id = v_cohort_id
          AND tenant_code = v_tenant
          AND task_reward_code = v_task_reward_code
          AND delete_nbr = 0;

        IF FOUND THEN
            RAISE NOTICE '✔ Soft deleted cohort_tenant_task_reward for tenant %, reward %',
                         v_tenant, v_task_reward_code;
        ELSE
            RAISE NOTICE 'ℹ No cohort_tenant_task_reward to delete for tenant %', v_tenant;
        END IF;


        -- ======================================================
        -- Soft Delete: task.task_reward
        -- ======================================================
        UPDATE task.task_reward
        SET delete_nbr = 1,
            update_ts = NOW(),
            update_user = 'SYSTEM-ROLLBACK'
        WHERE task_reward_code = v_task_reward_code
          AND delete_nbr = 0;

        IF FOUND THEN
            RAISE NOTICE '✔ Soft deleted task_reward % for tenant %', v_task_reward_code, v_tenant;
        ELSE
            RAISE NOTICE 'ℹ No task_reward row soft deleted for tenant %', v_tenant;
        END IF;

    END LOOP;

    RAISE NOTICE 'Rollback script completed successfully.';

END $$;
