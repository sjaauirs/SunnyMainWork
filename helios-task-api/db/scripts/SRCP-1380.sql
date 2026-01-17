
ALTER TABLE task.consumer_task
  ADD column IF NOT EXISTS parent_consumer_task_id bigint NULL
  CONSTRAINT fk_parent_consumer_task_id REFERENCES task.consumer_task(consumer_task_id);
