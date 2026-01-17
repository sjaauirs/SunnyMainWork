DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.tables 
        WHERE table_schema = 'task'
        AND table_name = 'consumer_task'
    ) THEN
        GRANT SELECT ON TABLE task.consumer_task TO group_readonly;
    END IF;
END $$;