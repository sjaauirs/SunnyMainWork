-- SOCT-1601: This script updates the task_description in task.task_detail TABLE
-- Replace the input parameters before executing the script

DO $$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>';  -- Replace with actual tenant code
    v_task_header TEXT := 'Get your z''s';      
    v_update_description TEXT := 
        '[{"type":"paragraph","data":{"text":"Experts recommend 7 to 9 hours of sleep nightly for a healthy mind and body. Use your preferred device or other tool to track your sleep. Next, log at least 7 hours on at least four or more nights per week and earn rewards for the month when you hit 20 nights. \n\nFor consistent sleep try these simple tips:"}},{"type":"list","data":{"style":"unordered","items":["Establish a set bed time and wake time.","Go to bed and wake up within 30 minutes of your set bed time and wake time.","Keep your room dark and cool."]}}]';
    v_language_code TEXT := 'en-US';
    v_task_detail_id BIGINT;
BEGIN
    -- Try to find task_detail_id
    SELECT task_detail_id
    INTO v_task_detail_id
    FROM task.task_detail
    WHERE task_header = v_task_header
      AND tenant_code = v_tenant_code
      AND language_code = v_language_code
      AND delete_nbr = 0
    LIMIT 1;

    -- Fail immediately if not found
    IF v_task_detail_id IS NULL THEN
        RAISE EXCEPTION 'No task_detail found for tenant: %, header: "%"', v_tenant_code, v_task_header;
    END IF;

    -- Update the matching record
    UPDATE task.task_detail
    SET task_description = v_update_description,
        update_ts = now(),
        update_user = 'SYSTEM'
    WHERE task_detail_id = v_task_detail_id;

    RAISE NOTICE 'Updated task_detail_id: %, header: "%", tenant: "%"', v_task_detail_id, v_task_header, v_tenant_code;
END $$;
