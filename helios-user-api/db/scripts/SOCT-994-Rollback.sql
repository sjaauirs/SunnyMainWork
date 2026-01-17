-- Drop the composite index if it exists
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM pg_indexes 
        WHERE schemaname = 'huser'
          AND indexname = 'idx_consumer_history_consumer_tenant'
    ) THEN
        EXECUTE 'DROP INDEX huser.idx_consumer_history_consumer_tenant';
    END IF;
END
$$;

DROP TABLE IF EXISTS huser.consumer_history;
