DO $$ 
DECLARE 
    updated_count INTEGER;
BEGIN
    -- Update task_reward_config_json for all records where is_collection = true
    UPDATE task.task_reward
    SET task_reward_config_json = 
        CASE 
            WHEN task_reward_config_json IS NULL OR task_reward_config_json = '{}'::jsonb THEN 
                '{"collectionConfig": {"flattenTasks": true, "includeInAllAvailableTasks": false}}'::jsonb
            WHEN task_reward_config_json ? 'collectionConfig' THEN 
                jsonb_set(
                    jsonb_set(task_reward_config_json, '{collectionConfig,flattenTasks}', 'true'::jsonb),
                    '{collectionConfig,includeInAllAvailableTasks}', 'false'::jsonb
                )
            ELSE 
                jsonb_set(task_reward_config_json, '{collectionConfig}', 
                          task_reward_config_json->'collectionConfig' || 
                          '{"flattenTasks": true, "includeInAllAvailableTasks": false}'::jsonb)
        END
    WHERE is_collection = true
    RETURNING 1 INTO updated_count;

    -- Check if any rows were updated
    IF updated_count > 0 THEN
        RAISE NOTICE 'Update successful: % records updated.', updated_count;
    ELSE
        RAISE NOTICE 'No matching records found or already up-to-date.';
    END IF;
EXCEPTION 
    WHEN OTHERS THEN 
        RAISE NOTICE 'Update failed: %', SQLERRM;
END $$;