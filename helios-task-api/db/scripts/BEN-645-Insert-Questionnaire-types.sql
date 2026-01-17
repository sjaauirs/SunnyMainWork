-- ============================================================================
-- 🚀 Script: Upsert Task Types (SURVEY, FEEDBACK)
-- 📌 Purpose: Insert or update task types by task_type_Code + delete_nbr=0
-- Author : Siva Krishna
-- Jira task: BEN-645
-- ============================================================================

DO $$
DECLARE
    v_survey_task_type_code   TEXT := 'tty-86398dc3a77d4a3db7922e57b5b6d73c'; -- task_type_code for 'SURVEY'
    v_feedback_task_type_code TEXT := 'tty-9fcbfa97e2e24e37910dd69d3370eded'; -- task_type_code for 'FEEDBACK'
BEGIN
    -- ==========================================================
    -- Upsert Task Type: SURVEY
    -- ==========================================================
    IF EXISTS (
        SELECT 1
        FROM task.task_type
        WHERE task_type_code = v_survey_task_type_code
          AND delete_nbr = 0
    ) THEN
        UPDATE task.task_type
        SET task_type_description = 'Task type for questionnaires and surveys',
            update_ts = now(),
            update_user = 'system'
        WHERE task_type_code = v_survey_task_type_code
          AND delete_nbr = 0;

        RAISE NOTICE 'Updated Task Type: SURVEY';
    ELSE
        INSERT INTO task.task_type (
            task_type_code,
            task_type_name,
            task_type_description,
            create_ts,
            create_user,
            update_ts,
            update_user,
            delete_nbr,
            is_subtask
        )
        VALUES (
            v_survey_task_type_code,     -- task_type_code
            'SURVEY',                    -- task_type_name
            'Task type for questionnaires and surveys',
            now(),
            'system',
            NULL,
            NULL,
            0,
            false
        );

        RAISE NOTICE 'Inserted Task Type: SURVEY';
    END IF;

    -- ==========================================================
    -- Upsert Task Type: FEEDBACK
    -- ==========================================================
    IF EXISTS (
        SELECT 1
        FROM task.task_type
        WHERE task_type_code = v_feedback_task_type_code
          AND delete_nbr = 0
    ) THEN
        UPDATE task.task_type
        SET task_type_description = 'Task type for collecting user feedback',
            update_ts = now(),
            update_user = 'system'
        WHERE task_type_code = v_feedback_task_type_code
          AND delete_nbr = 0;

        RAISE NOTICE 'Updated Task Type: FEEDBACK';
    ELSE
        INSERT INTO task.task_type (
            task_type_code,
            task_type_name,
            task_type_description,
            create_ts,
            create_user,
            update_ts,
            update_user,
            delete_nbr,
            is_subtask
        )
        VALUES (
            v_feedback_task_type_code,   -- task_type_code
            'FEEDBACK',                  -- task_type_name
            'Task type for collecting user feedback',
            now(),
            'system',
            NULL,
            NULL,
            0,
            false
        );

        RAISE NOTICE 'Inserted Task Type: FEEDBACK';
    END IF;
END
$$ LANGUAGE plpgsql;