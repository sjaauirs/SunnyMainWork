CREATE UNIQUE INDEX IF NOT EXISTS idx_task_reward_3
    ON task.task_reward (task_id, tenant_code, delete_nbr);