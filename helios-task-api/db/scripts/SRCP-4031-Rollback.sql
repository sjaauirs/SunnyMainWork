-- Check and Drop is_collection column if it does not exist
DO $$ 
DECLARE 
    column_exists BOOLEAN;
BEGIN
    -- Check if the column exists
    SELECT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'task_reward' 
        AND table_schema = 'task' 
        AND column_name = 'is_collection'
    ) INTO column_exists;

    -- Drop column if it exists
    IF column_exists THEN
        ALTER TABLE task.task_reward 
        DROP COLUMN is_collection;
        RAISE NOTICE 'Column is_collection dropped successfully from task.task_reward.';
    ELSE
        RAISE NOTICE 'Column is_collection does not exist in task.task_reward.';
    END IF;
END $$;

-- Check and Drop unique index idx_unique_child_code_delete_nbr if it exists
DO $$ 
DECLARE 
    index_exists BOOLEAN;
BEGIN
    SELECT EXISTS (
        SELECT 1 FROM pg_indexes 
        WHERE schemaname = 'task' AND tablename = 'task_reward_collection' 
        AND indexname = 'idx_unique_child_code_delete_nbr'
    ) INTO index_exists;

    IF index_exists THEN
        DROP INDEX task.idx_unique_child_code_delete_nbr;
        RAISE NOTICE 'Index idx_unique_child_code_delete_nbr dropped successfully.';
    ELSE
        RAISE NOTICE 'Index idx_unique_child_code_delete_nbr does not exist.';
    END IF;
END $$;

-- Check and Drop unique index idx_unique_parent_child_delete_nbr if it exists
DO $$ 
DECLARE 
    index_exists BOOLEAN;
BEGIN
    SELECT EXISTS (
        SELECT 1 FROM pg_indexes 
        WHERE schemaname = 'task' AND tablename = 'task_reward_collection' 
        AND indexname = 'idx_unique_parent_child_delete_nbr'
    ) INTO index_exists;

    IF index_exists THEN
        DROP INDEX task.idx_unique_parent_child_delete_nbr;
        RAISE NOTICE 'Index idx_unique_parent_child_delete_nbr dropped successfully.';
    ELSE
        RAISE NOTICE 'Index idx_unique_parent_child_delete_nbr does not exist.';
    END IF;
END $$;

-- Check and Drop table task.task_reward_collection if it exists
DO $$ 
DECLARE 
    table_exists BOOLEAN;
BEGIN
    SELECT EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_schema = 'task' AND table_name = 'task_reward_collection'
    ) INTO table_exists;

    IF table_exists THEN
        DROP TABLE task.task_reward_collection;
        RAISE NOTICE 'Table task.task_reward_collection dropped successfully.';
    ELSE
        RAISE NOTICE 'Table task.task_reward_collection does not exist.';
    END IF;
END $$;