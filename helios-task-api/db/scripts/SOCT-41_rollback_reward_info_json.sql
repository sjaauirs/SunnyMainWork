-- Rollback Step 1: Set reward_info_json to NULL
UPDATE task.consumer_task
SET reward_info_json = NULL
WHERE reward_info_json IS NOT NULL;

-- Rollback Step 2: Drop the reward_info_json column (only if it exists)
ALTER TABLE task.consumer_task
DROP COLUMN IF EXISTS reward_info_json;
