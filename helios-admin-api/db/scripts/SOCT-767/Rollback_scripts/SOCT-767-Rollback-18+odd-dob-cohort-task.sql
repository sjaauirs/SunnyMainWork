-- ============================================================================
-- This rollback script performs the following operations to revert the insert:
-- 
-- 1. Identifies and deletes the cohort-to-task-reward link from 
--    `cohort.cohort_tenant_task_reward`, if it exists.
-- 2. Soft Deletes the task reward from `task.task_reward`, if it was previously created.
-- 3. Soft Deletes the task detail from `task.task_detail` for the specific task and tenant.
-- 4. Soft Deletes the task from `task.task`, if it exists.
-- 5. Soft Deletes the cohort from `cohort.cohort`, if it was inserted.
--
-- Each delete operation is preceded by an existence check using the record's ID.
-- Informative RAISE NOTICE statements are added to confirm whether each record
-- was successfully deleted or skipped because it was not found.
-- 
-- The rollback is executed within a block that handles and logs any errors.

DO $$
DECLARE
    -- Input values matching the inserted script
    v_cohort_name TEXT := 'adult18up+odd_dob';
    v_task_name TEXT := 'Play Daily Trivia';
    v_task_external_code TEXT := 'play_dail_heal_triv';
    v_tenant_code TEXT := 'ten-153bd6c47ebe4673a75c71faa22b9eb6';

    -- Variables to hold IDs for deletion
    v_cohort_id BIGINT;
    v_task_id BIGINT;
    v_task_detail_id BIGINT;
    v_task_reward_code TEXT;
    v_task_reward_id BIGINT;
    v_cohort_tenant_task_reward_id BIGINT;

BEGIN
    -- ============================================================================
    -- Step 1: Identify cohort by name
    -- ============================================================================
    SELECT cohort_id INTO v_cohort_id
    FROM cohort.cohort
    WHERE cohort_name = v_cohort_name AND delete_nbr = 0;

    -- ============================================================================
    -- Step 2: Identify task by name
    -- ============================================================================
    SELECT task_id INTO v_task_id
    FROM task.task
    WHERE task_name = v_task_name AND delete_nbr = 0;

    -- ============================================================================
    -- Step 3: Identify task detail by task and tenant
    -- ============================================================================
    SELECT task_detail_id INTO v_task_detail_id
    FROM task.task_detail
    WHERE task_id = v_task_id AND tenant_code = v_tenant_code AND delete_nbr = 0;

    -- ============================================================================
    -- Step 4: Identify task reward by task and tenant
    -- ============================================================================
    SELECT task_reward_code, task_reward_id INTO v_task_reward_code, v_task_reward_id
    FROM task.task_reward
    WHERE task_id = v_task_id AND tenant_code = v_tenant_code AND delete_nbr = 0;

    -- ============================================================================
    -- Step 5: Identify cohort-task-reward link
    -- ============================================================================
    SELECT cohort_tenant_task_reward_id INTO v_cohort_tenant_task_reward_id
    FROM cohort.cohort_tenant_task_reward
    WHERE cohort_id = v_cohort_id AND tenant_code = v_tenant_code 
      AND task_reward_code = v_task_reward_code AND delete_nbr = 0;

    -- ============================================================================
    -- Step 6: Delete from cohort.cohort_tenant_task_reward
    -- ============================================================================
    IF v_cohort_tenant_task_reward_id IS NOT NULL THEN
        UPDATE cohort.cohort_tenant_task_reward SET delete_nbr = cohort_tenant_task_reward_id, update_ts = NOW(), update_user = 'ROLLBACK'
        WHERE cohort_tenant_task_reward_id = v_cohort_tenant_task_reward_id;
        RAISE NOTICE '✅ Deleted from cohort.cohort_tenant_task_reward: ID = %', v_cohort_tenant_task_reward_id;
    ELSE
        RAISE NOTICE 'ℹ️ No matching record found in cohort.cohort_tenant_task_reward to delete.';
    END IF;

    -- ============================================================================
    -- Step 7: Delete from task.task_reward
    -- ============================================================================
    IF v_task_reward_id IS NOT NULL THEN
        UPDATE task.task_reward SET delete_nbr = task_reward_id, update_ts = NOW(), update_user = 'ROLLBACK'
        WHERE task_reward_id = v_task_reward_id;
        RAISE NOTICE '✅ Deleted from task.task_reward: ID = %', v_task_reward_id;
    ELSE
        RAISE NOTICE 'ℹ️ No matching record found in task.task_reward to delete.';
    END IF;

    -- ============================================================================
    -- Step 8: Delete from task.task_detail
    -- ============================================================================
    IF v_task_detail_id IS NOT NULL THEN
        UPDATE task.task_detail SET delete_nbr = task_detail_id, update_ts = NOW(), update_user = 'ROLLBACK'
        WHERE task_detail_id = v_task_detail_id;
        RAISE NOTICE '✅ Deleted from task.task_detail: ID = %', v_task_detail_id;
    ELSE
        RAISE NOTICE 'ℹ️ No matching record found in task.task_detail to delete.';
    END IF;

    -- ============================================================================
    -- Step 9: Delete from task.task
    -- ============================================================================
    IF v_task_id IS NOT NULL THEN
        UPDATE task.task SET delete_nbr = task_id, update_ts = NOW(), update_user = 'ROLLBACK'
        WHERE task_id = v_task_id;
        RAISE NOTICE '✅ Deleted from task.task: ID = %', v_task_id;
    ELSE
        RAISE NOTICE 'ℹ️ No matching record found in task.task to delete.';
    END IF;

    -- ============================================================================
    -- Step 10: Delete from cohort.cohort
    -- ============================================================================
    IF v_cohort_id IS NOT NULL THEN
        UPDATE cohort.cohort SET delete_nbr = cohort_id, update_ts = NOW(), update_user = 'ROLLBACK'
        WHERE cohort_id = v_cohort_id;
        RAISE NOTICE '✅ Deleted from cohort.cohort: ID = %', v_cohort_id;
    ELSE
        RAISE NOTICE 'ℹ️ No matching record found in cohort.cohort to delete.';
    END IF;

    -- ============================================================================
    -- Completion
    -- ============================================================================
    RAISE NOTICE '🎯 Rollback script completed successfully.';

EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE '❌ An error occurred during rollback: %', SQLERRM;
        RAISE EXCEPTION '⚠️ Rollback failed.';
END $$;
