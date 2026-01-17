ALTER TABLE task.task_reward
ADD COLUMN IF NOT EXISTS valid_start_ts timestamp;

update task.task_reward set valid_start_ts='2023-01-01 00:00:00', expiry='2100-01-01 00:00:00';
update task.task_reward set valid_start_ts='2023-11-01 00:00:00', expiry='2023-11-30 23:59:59' where task_id=(select task_id from task.task_detail where task_header ilike '%novem%trivia%')
