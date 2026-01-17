ALTER TABLE task.task_reward 
ADD COLUMN IF NOT EXISTS confirm_report BOOLEAN not null default false;