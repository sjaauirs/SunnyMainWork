-- ============================================================================
-- This rollback script reverses the operations performed by the original script:
-- 1. Unlinks the cohort from the task reward in `cohort.cohort_tenant_task_reward`.
-- 2. Deletes the corresponding task reward from `task.task_reward`.
-- 3. Deletes the task detail entry from `task.task_detail`.
-- 4. Deletes the task from `task.task`.
-- 5. Deletes the cohort from `cohort.cohort`.
--
-- The deletions are performed in reverse dependency order to maintain integrity.
-- It uses cohort name, task name, and tenant code to identify records created.
-- Informative RAISE NOTICE messages are included for traceability.
-- ============================================================================

DO $$
DECLARE
    v_cohort_name TEXT := 'sweepstakes_winners';
    v_task_detail_name TEXT := 'Sweepstakes Winner';
    v_task_external_code TEXT := 'swee_winn';
    v_tenant_code TEXT := 'ten-ecada21e57154928a2bb959e8365b8b4';

    v_cohort_id BIGINT;
    v_task_id BIGINT;
    v_task_reward_code TEXT;
BEGIN
    -- Get cohort_id
    SELECT cohort_id INTO v_cohort_id
    FROM cohort.cohort
    WHERE cohort_name = v_cohort_name AND delete_nbr = 0;

    -- Get task_id
    SELECT task_id INTO v_task_id
    FROM task.task
    WHERE task_name = v_cohort_name AND delete_nbr = 0;

    -- Get task_reward_code
    SELECT task_reward_code INTO v_task_reward_code
    FROM task.task_reward
    WHERE task_id = v_task_id AND tenant_code = v_tenant_code AND delete_nbr = 0;

    -- Delete from cohort.cohort_tenant_task_reward
    DELETE FROM cohort.cohort_tenant_task_reward
    WHERE cohort_id = v_cohort_id
      AND task_reward_code = v_task_reward_code
      AND tenant_code = v_tenant_code;

    RAISE NOTICE 'Deleted link from cohort.cohort_tenant_task_reward for cohort_id=%', v_cohort_id;

    -- Delete from task.task_reward
    DELETE FROM task.task_reward
    WHERE task_id = v_task_id
      AND tenant_code = v_tenant_code
      AND task_reward_code = v_task_reward_code;

    RAISE NOTICE 'Deleted from task.task_reward for task_id=%', v_task_id;

    -- Delete from task.task_detail
    DELETE FROM task.task_detail
    WHERE task_id = v_task_id
      AND tenant_code = v_tenant_code;

    RAISE NOTICE 'Deleted from task.task_detail for task_id=%', v_task_id;

    -- Delete from task.task
    DELETE FROM task.task
    WHERE task_id = v_task_id;

    RAISE NOTICE 'Deleted from task.task: task_id=%', v_task_id;

    -- Delete from cohort.cohort
    DELETE FROM cohort.cohort
    WHERE cohort_id = v_cohort_id;

    RAISE NOTICE 'Deleted from cohort.cohort: cohort_id=%', v_cohort_id;

    RAISE NOTICE 'Rollback script completed successfully.';

EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE 'Error occurred during rollback: %', SQLERRM;
        RAISE EXCEPTION 'Rollback failed.';
END $$;
