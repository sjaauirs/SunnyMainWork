DO $$ 
DECLARE 
    adventure_rec RECORD;
    cohort_rec RECORD;
    new_task_id BIGINT;
    new_task_reward_id BIGINT;
    ref_tenant_code TEXT;
    new_task_reward_code TEXT;
    child_task_reward_rec RECORD;
    selected_child_rewards INT := 0;
    task_external_code TEXT;
BEGIN
    -- Fetch the tenant_code dynamically
    SELECT tenant_code INTO ref_tenant_code 
    FROM tenant.tenant 
    WHERE tenant_code = 'ten-ecada21e57154928a2bb959e8365b8b4' 
    AND delete_nbr = 0;

    -- Exit if no valid tenant_code is found
    IF ref_tenant_code IS NULL THEN
        RAISE NOTICE 'No tenant_code found, exiting...';
        RETURN;
    END IF;
    
    RAISE NOTICE 'Processing tenant_code: %', ref_tenant_code;

    -- Loop through each adventure
    FOR adventure_rec IN 
        SELECT 
            adventure_id,
            TRIM(BOTH '"' FROM jsonb_path_query_first(adventure_config_json, '$.cohorts[0]')::TEXT) AS cohort_name,
			TRIM(BOTH '"' FROM regexp_replace(jsonb_path_query_first(adventure_config_json, '$.cohorts[0]')::TEXT, '[:_]', ' ', 'g')) AS adventure_name
        FROM task.adventure 
        WHERE adventure_id IN (
            SELECT adventure_id 
            FROM task.tenant_adventure 
            WHERE tenant_code = ref_tenant_code
            AND delete_nbr = 0
        )
    LOOP
        -- Handle NULL adventure names
        IF adventure_rec.adventure_name IS NULL THEN
            adventure_rec.adventure_name := 'Unknown Adventure ' || adventure_rec.adventure_id;
        END IF;

        -- Generate task_external_code (replace spaces with underscores)
        task_external_code := replace(adventure_rec.adventure_name, ' ', '_');

        RAISE NOTICE 'Inserting task for adventure: % with external code: %', adventure_rec.adventure_name, task_external_code;

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
        )
        VALUES (
            1, 
           'tsk-' || gen_random_uuid()::TEXT,  -- Generate UUID for task_code
            adventure_rec.adventure_name, 
            NOW(), 
            null, 
            'SYSTEM', 
            null, 
            0, 
            FALSE, 
            FALSE, 
            NULL, 
            FALSE
        )
        RETURNING task_id INTO new_task_id;

        RAISE NOTICE 'Inserted task with ID: %', new_task_id;

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
        )
        VALUES (
            new_task_id, 
            'en-US', 
            'Header for ' || adventure_rec.adventure_name, 
            'Description for ' || adventure_rec.adventure_name, 
            1, 
            NOW(), 
            null, 
            'system', 
            null, 
            0, 
            'Enroll now', 
            ref_tenant_code
        );

        RAISE NOTICE 'Inserted task_detail for task_id: %', new_task_id;

        -- Generate new task_reward_code
        new_task_reward_code := 'trw-' || gen_random_uuid()::TEXT;

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
            task_reward_config_json, 
            is_collection
        )
        VALUES (
            new_task_id, 
            1, 
            ref_tenant_code, 
            new_task_reward_code, 
            '{}'::jsonb, 
            NULL, 
            NULL, 
            NOW() + INTERVAL '1 year', 
            1, 
            NOW(), 
            null, 
            'SYSTEM', 
            null, 
            0, 
            NULL, 
            task_external_code, 
            NOW(), 
            FALSE, 
            NULL, 
            TRUE, 
            NULL, 
            FALSE, 
            '{
                "collectionConfig": {
                    "flattenTasks": true,
                    "includeInAllAvailableTasks": false
                }
            }'::jsonb, 
            TRUE
        )
        RETURNING task_reward_id INTO new_task_reward_id; -- Capture the new task_reward_id

        RAISE NOTICE 'Inserted task_reward with ID: % and task_reward_code: %', new_task_reward_id, new_task_reward_code;

        -- Insert into cohort.cohort_tenant_task_reward
        FOR cohort_rec IN 
            SELECT cohort_id 
            FROM cohort.cohort 
		    WHERE TRIM(cohort_name) ILIKE '%' || TRIM(adventure_rec.cohort_name) || '%'   
            AND delete_nbr = 0
        LOOP
            INSERT INTO cohort.cohort_tenant_task_reward (
                cohort_id, 
                tenant_code, 
                task_reward_code, 
                recommended, 
                priority, 
                create_ts, 
                update_ts, 
                create_user, 
                update_user, 
                delete_nbr
            )
            VALUES (
                cohort_rec.cohort_id, 
                ref_tenant_code, 
                new_task_reward_code, 
                FALSE,  
                1,  
                NOW(), 
                null, 
                'SYSTEM', 
                null, 
                0
            );

            RAISE NOTICE 'Inserted cohort_tenant_task_reward for cohort_id: % with task_reward_code: %', cohort_rec.cohort_id, new_task_reward_code;
        END LOOP;

        -- Insert into task.task_reward_collection (select 3 child tasks)
        selected_child_rewards := 0;
        FOR child_task_reward_rec IN 
            SELECT task_reward_id 
            FROM task.task_reward 
            WHERE task_id IN (
                SELECT task_id 
                FROM task.task 
                WHERE task_name ILIKE '%sample Task%' 
                AND delete_nbr = 0
            )
            AND tenant_code = ref_tenant_code
            ORDER BY RANDOM()  
            LIMIT 3  
        LOOP
            IF selected_child_rewards < 3 THEN
                INSERT INTO task.task_reward_collection (
                    parent_task_reward_id, 
                    child_task_reward_id, 
                    unique_child_code, 
                    config_json, 
                    create_ts, 
                    update_ts, 
                    create_user, 
                    update_user, 
                    delete_nbr
                )
                VALUES (
                    new_task_reward_id, 
                    child_task_reward_rec.task_reward_id, 
                    gen_random_uuid()::TEXT,  
                    '{}'::jsonb,  
                    NOW(), 
                    null, 
                    'SYSTEM', 
                    null, 
                    0
                );

                selected_child_rewards := selected_child_rewards + 1;
                RAISE NOTICE 'Inserted task_reward_collection (child task ID: %) under parent task_reward_id: %', child_task_reward_rec.task_reward_id, new_task_reward_id;
            END IF;
        END LOOP;

    END LOOP;
END $$;
