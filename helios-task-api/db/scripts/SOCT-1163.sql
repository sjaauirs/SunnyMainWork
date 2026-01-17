DO $$
BEGIN
    -- Check if the column does NOT exist
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns 
        WHERE table_schema = 'task'
          AND table_name = 'task_reward'
          AND column_name = 'completion_eligibility_json'
    ) THEN
        ALTER TABLE task.task_reward
        ADD COLUMN completion_eligibility_json JSONB NULL;
    END IF;
END $$;
