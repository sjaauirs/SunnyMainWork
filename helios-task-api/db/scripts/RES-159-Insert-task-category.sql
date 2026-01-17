-- Jira : RES-159
-- purpose : Upsert new task_category 

DO
$$
DECLARE
    v_task_category_code CONSTANT VARCHAR := 'tcc-f2aa20d98d6e4856bd02cdfcd650b59d';
    v_task_category_name CONSTANT VARCHAR := 'Condition Care';
    v_description CONSTANT VARCHAR := 'Condition Care';
    v_exists BOOLEAN;
BEGIN
    -- Check if record exists
    SELECT EXISTS (
        SELECT 1
        FROM task.task_category
        WHERE task_category_code = v_task_category_code
          AND task_category_name = v_task_category_name
          AND delete_nbr = 0
    ) INTO v_exists;

    IF v_exists THEN
        -- Update existing record
        UPDATE task.task_category
        SET task_category_description = v_description,
            update_user = 'SYSTEM',
            update_ts = NOW()
        WHERE task_category_code = v_task_category_code
          AND task_category_name = v_task_category_name
          AND delete_nbr = 0;

        RAISE NOTICE '✅ Updated existing Task Category: %', v_task_category_name;
    ELSE
        -- Insert new record
        INSERT INTO task.task_category (
            task_category_code,
            task_category_description,
            create_ts,
            update_ts,
            create_user,
            update_user,
            delete_nbr,
            task_category_name
        )
        VALUES (
            v_task_category_code,
            v_description,
            NOW(),
            NULL,
            'SYSTEM',
            NULL,
            0,
            v_task_category_name
        );

        RAISE NOTICE '✅ Inserted new Task Category: %', v_task_category_name;
    END IF;
END
$$;
