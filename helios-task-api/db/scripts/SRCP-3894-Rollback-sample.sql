DO $$ 
DECLARE 
    ref_task_id BIGINT;
    ref_task_reward_id BIGINT;
    ref_tenant_code TEXT := 'ten-ecada21e57154928a2bb959e8365b8b4'; -- Input Tenant Code
    task_name_to_delete TEXT := 'Upload product image'; -- Input Task Name
BEGIN 
    -- Fetch task_id for the given task_name
    SELECT task_id INTO ref_task_id 
    FROM task.task 
    WHERE task_name = task_name_to_delete 
    AND tenant_code = ref_tenant_code;

    IF ref_task_id IS NOT NULL THEN
        -- Fetch task_reward_id for the found task
        SELECT task_reward_id INTO ref_task_reward_id 
        FROM task.task_reward 
        WHERE task_id = ref_task_id AND delete_nbr=0;

        -- Delete from task_reward table
        DELETE FROM task.task_reward 
        WHERE task_id = ref_task_id AND delete_nbr=0;

        -- Delete from task_detail table (if applicable)
        DELETE FROM task.task_detail 
        WHERE task_id = ref_task_id AND delete_nbr=0;

        -- Delete from task table
        DELETE FROM task.task 
        WHERE task_id = ref_task_id AND delete_nbr=0;

        -- Log success message
        RAISE NOTICE 'Reverted changes: Deleted task %, task_reward %', ref_task_id, ref_task_reward_id;
    ELSE
        RAISE NOTICE 'No matching task found for rollback with name: %', task_name_to_delete;
    END IF;
END $$;
