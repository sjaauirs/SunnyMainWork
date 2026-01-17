DO $$ 
DECLARE 
    ref_task_id BIGINT;
    ref_task_reward_id BIGINT;
    ref_task_type_id BIGINT;
    ref_reward_type_id BIGINT;
    ref_task_code TEXT;
    ref_task_reward_code TEXT;
    ref_tenant_code TEXT := 'ten-ecada21e57154928a2bb959e8365b8b4'; -- Input Tenant Code
BEGIN 
    -- Fetch task_type_id for 'HEALTH_ACTIONS'
    SELECT task_type_id INTO ref_task_type_id 
    FROM task.task_type 
    WHERE task_type_name = 'HEALTH_ACTIONS';

    IF ref_task_type_id IS NULL THEN
        RAISE EXCEPTION 'Task Type ID not found for HEALTH_ACTIONS';
    END IF;

    -- Fetch reward_type_id for 'MONETARY_DOLLARS'
    SELECT reward_type_id INTO ref_reward_type_id 
    FROM task.reward_type 
    WHERE reward_type_name = 'MONETARY_DOLLARS';

    IF ref_reward_type_id IS NULL THEN
        RAISE EXCEPTION 'Reward Type ID not found for MONETARY_DOLLARS';
    END IF;

    BEGIN -- Start Transaction Block
        -- Generate unique task_code and task_reward_code
        ref_task_code := 'tsk-' || REPLACE(gen_random_uuid()::TEXT, '-', '');
        ref_task_reward_code := 'trw-' || REPLACE(gen_random_uuid()::TEXT, '-', '');

        -- Insert into task.task
        INSERT INTO task.task (
            task_type_id,
            task_code,
            task_name,
            create_ts,
            update_ts,
            create_user,
            update_user,
            delete_nbr,
            self_report,
            confirm_report,
            task_category_id,
            is_subtask
        ) VALUES (
            ref_task_type_id,
            ref_task_code,
            'Upload Product Image',  -- Task Name
            NOW(),
            NULL,
            'SYSTEM',
            NULL,
            0,
            FALSE,
            FALSE,
            NULL,
            FALSE
        ) RETURNING task_id INTO ref_task_id;

        -- Insert into task.task_detail
        INSERT INTO task.task_detail (
            task_id,
            language_code,
            task_header,
            task_description,
            terms_of_service_id,
            create_ts,
            update_ts,
            create_user,
            update_user,
            delete_nbr,
            task_cta_button_text,
            tenant_code
        ) VALUES (
            ref_task_id,
            'en-us',  -- Language
            'Upload Product Image',  -- Task Header
            'Upload image to win prizes',  -- Task Description
            1,  -- Terms of Service ID
            NOW(),
            NULL,
            'SYSTEM',
            NULL,
            0,
            'Upload Image',
            ref_tenant_code
        );

        -- Insert into task.task_reward
        INSERT INTO task.task_reward (
            task_id,
            reward_type_id,
            tenant_code,
            task_reward_code,
            reward,
            min_task_duration,
            max_task_duration,
            expiry,
            priority,
            create_ts,
            update_ts,
            create_user,
            update_user,
            delete_nbr,
            task_action_url,
            task_external_code,
            valid_start_ts,
            is_recurring,
            recurrence_definition_json,
            self_report,
            task_completion_criteria_json,
            confirm_report,
            task_reward_config_json
        ) VALUES (
            ref_task_id,
            ref_reward_type_id,
            ref_tenant_code,
            ref_task_reward_code,
            jsonb_build_object(
                'rewardType', 'MONETARY_DOLLARS',
                'rewardAmount', '25',
                'membershipType', NULL
            ),
            0,
            0,
            '2025-12-31',  -- Set expiry date
            0,
            NOW(),
            NULL,
            'SYSTEM',
            NULL,
            0,
            NULL,
            'sample_image_task',
            NOW(),
            FALSE,
            NULL,
            TRUE,
            jsonb_build_object(
                'imageCriteria', jsonb_build_object('requiredImageCount', 2)
            ),
            FALSE,
            NULL
        ) RETURNING task_reward_id INTO ref_task_reward_id;

       
        -- Log successful insertion
        RAISE NOTICE 'Inserted task % (type: %) and task reward % for tenant %', 
            ref_task_code, ref_task_type_id, ref_task_reward_code, ref_tenant_code;

    EXCEPTION
        WHEN OTHERS THEN
            -- Rollback transaction in case of failure
            ROLLBACK;
            RAISE WARNING 'Transaction failed for tenant %: %', ref_tenant_code, SQLERRM;
    END; -- End transaction block
    
    -- Final log message
    RAISE NOTICE 'Task insertion process completed.';
END $$;
