-- add a new column in task.task_reward if not exists task_reward_config_json
ALTER TABLE task.task_reward 
ADD COLUMN IF NOT EXISTS task_reward_config_json JSONB;