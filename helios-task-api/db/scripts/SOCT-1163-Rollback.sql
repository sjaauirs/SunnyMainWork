DO $$
BEGIN
    -- Check if the column exists
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns 
        WHERE table_schema = 'task'
          AND table_name = 'task_reward'
          AND column_name = 'completion_eligibility_json'
    ) THEN
        ALTER TABLE task.task_reward
        DROP COLUMN completion_eligibility_json;
    END IF;
END $$;
