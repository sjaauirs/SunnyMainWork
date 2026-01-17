ALTER TABLE task.task_reward 
ADD COLUMN IF NOT EXISTS task_completion_criteria_json jsonb;