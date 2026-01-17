DO $$
DECLARE 
    ref_tenant_code TEXT;
    new_task_reward_codes TEXT[];
    new_task_ids BIGINT[];
    time_threshold INTERVAL := '1440 minutes'; -- Adjust as needed
BEGIN
    -- Fetch the tenant_code dynamically
    SELECT tenant_code INTO ref_tenant_code 
    FROM tenant.tenant 
    WHERE tenant_code = 'ten-ecada21e57154928a2bb959e8365b8b4' 
    AND delete_nbr = 0;

    -- Exit if no valid tenant_code is found
    IF ref_tenant_code IS NULL THEN
        RAISE NOTICE 'No tenant_code found, exiting rollback...';
        RETURN;
    END IF;

    -- Retrieve only recently inserted task_reward_codes
    SELECT array_agg(task_reward_code) INTO new_task_reward_codes
    FROM task.task_reward
    WHERE tenant_code = ref_tenant_code
    AND create_ts >= NOW() - time_threshold;  -- Fetch last X minutes records

    -- If no records found, exit
    IF new_task_reward_codes IS NULL THEN
        RAISE NOTICE 'No recent task_reward records found, exiting rollback...';
        RETURN;
    END IF;

    -- Retrieve task_ids for deletion
    SELECT array_agg(task_id) INTO new_task_ids
    FROM task.task
    WHERE task_id IN (
        SELECT task_id FROM task.task_reward WHERE task_reward_code = ANY(new_task_reward_codes)
    );

    -- Step 1: Delete from task.task_reward_collection
    DELETE FROM task.task_reward_collection
    WHERE parent_task_reward_id IN (
        SELECT task_reward_id FROM task.task_reward WHERE task_reward_code = ANY(new_task_reward_codes)
    );

    -- Step 2: Delete from cohort.cohort_tenant_task_reward
    DELETE FROM cohort.cohort_tenant_task_reward
    WHERE task_reward_code = ANY(new_task_reward_codes);

    -- Step 3: Delete from task.task_reward
    DELETE FROM task.task_reward
    WHERE task_reward_code = ANY(new_task_reward_codes);

    -- Step 4: Delete from task.task_detail
    DELETE FROM task.task_detail
    WHERE task_id = ANY(new_task_ids);

    -- Step 5: Delete from task.task
    DELETE FROM task.task
    WHERE task_id = ANY(new_task_ids);

    RAISE NOTICE 'Rollback complete for recently inserted records.';

END $$;
