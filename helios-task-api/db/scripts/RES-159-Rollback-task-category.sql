-- Jira : RES-159
-- Purpose : Rollback for Upsert new task_category

DO
$$
DECLARE
    v_task_category_code CONSTANT VARCHAR := 'tcc-f2aa20d98d6e4856bd02cdfcd650b59d';
    v_deleted_count INT;
BEGIN
    -- Delete the task_category by code
    DELETE FROM task.task_category
    WHERE task_category_code = v_task_category_code;

    GET DIAGNOSTICS v_deleted_count = ROW_COUNT;

    IF v_deleted_count > 0 THEN
        RAISE NOTICE '♻️ Rollback successful: Deleted % record(s) with code %', v_deleted_count, v_task_category_code;
    ELSE
        RAISE NOTICE '⚠️ No records found to rollback for code %', v_task_category_code;
    END IF;
END
$$;
