ALTER TABLE task.task_reward ADD COLUMN IF NOT EXISTS is_recurring BOOLEAN not null default false;

ALTER TABLE task.task_reward ADD COLUMN IF NOT EXISTS recurrence_definition_json jsonb  null;

DROP INDEX IF EXISTS task.idx_task_id_consumer_code_delete_nbr;