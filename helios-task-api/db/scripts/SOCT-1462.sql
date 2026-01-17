-- ============================================================================
-- Purpose:
-- For given tenant codes, update child task_rewards linked to 'Strengthen your body'.
-- Set child.task_external_code = 'stre_your_body' only if different.
-- Update parent task_rewards linked via task_reward_collection, setting
-- includeInAllAvailableTasks to false only if currently true.
-- Logs counts of updated rows and handles exceptions.
-- ============================================================================

DO $$
DECLARE
    tenant_codes TEXT[] := ARRAY[
        '<KP-QA-TENANT-CODE>',
        '<KP-PROD-TENANT-CODE>'
    ];	--Add comma separated KP tenant codes
    
    child_update_count INT := 0;
    parent_update_count INT := 0;

BEGIN
    RAISE NOTICE 'Script started. Processing tenant codes: %', tenant_codes;

    -- Create temporary table to store child_task_reward_ids
    CREATE TEMP TABLE tmp_child_rewards(task_reward_id bigint) ON COMMIT DROP;

    -- Insert child task_reward ids into temp table
    INSERT INTO tmp_child_rewards(task_reward_id)
    SELECT tr.task_reward_id
    FROM task.task_reward tr
    JOIN task.task_detail td ON tr.task_id = td.task_id
    WHERE tr.delete_nbr = 0
      AND tr.tenant_code = ANY(tenant_codes)
      AND td.delete_nbr = 0
      AND td.task_header = 'Strengthen your body'
      AND td.tenant_code = ANY(tenant_codes);

    -- Step 3: Update child task_rewards with a new external code
    UPDATE task.task_reward tr
    SET task_external_code = 'stre_your_body'
    WHERE tr.task_reward_id IN (SELECT task_reward_id FROM tmp_child_rewards)
	AND tr.task_external_code IS DISTINCT FROM 'stre_your_body';

    GET DIAGNOSTICS child_update_count = ROW_COUNT;
    RAISE NOTICE '✔️ Updated % child task_reward rows.', child_update_count;

    -- Step 4: Update parent task_rewards' JSON config
    UPDATE task.task_reward parent
    SET task_reward_config_json = jsonb_set(
        parent.task_reward_config_json,
        '{collectionConfig,includeInAllAvailableTasks}',
        'false'::jsonb,
        true
    )
    FROM task.task_reward_collection trc
    INNER JOIN tmp_child_rewards cr ON trc.child_task_reward_id = cr.task_reward_id
    WHERE parent.task_reward_id = trc.parent_task_reward_id
	AND (parent.task_reward_config_json->'collectionConfig'->>'includeInAllAvailableTasks')::boolean = true;

    GET DIAGNOSTICS parent_update_count = ROW_COUNT;
    RAISE NOTICE '✔️ Updated % parent task_reward rows.', parent_update_count;

    RAISE NOTICE '✅ Script completed successfully.';

EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION '❌ An unexpected error occurred: %', SQLERRM;
END $$;
