-- Roll back if task_reward_config_json column exists
ALTER TABLE task.task_reward 
DROP COLUMN IF EXISTS task_reward_config_json;
