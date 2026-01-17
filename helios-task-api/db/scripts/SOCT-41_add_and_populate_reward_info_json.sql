-- Add the reward_info_json column if it doesn't exist
ALTER TABLE task.consumer_task
ADD COLUMN IF NOT EXISTS reward_info_json JSONB NULL;

-- Populate reward_info_json for active consumer tasks
UPDATE task.consumer_task ct
SET reward_info_json = jsonb_build_object(
    'currency', CASE 
                   WHEN rtype.reward_type_name IN ('MONETARY_DOLLARS', 'MEMBERSHIP_DOLLARS') THEN 'USD'
                   WHEN rtype.reward_type_name = 'SWEEPSTAKES_ENTRIES' THEN 'ENTRIES'
                   ELSE rtype.reward_type_name
               END,
    'rewardAmount', tr.reward ->> 'rewardAmount',
    'splitCurrency', false
)
FROM task.task_reward tr
JOIN task.reward_type rtype
  ON tr.reward_type_id = rtype.reward_type_id
WHERE ct.task_id = tr.task_id
  AND ct.tenant_code = tr.tenant_code
  AND ct.task_status = 'COMPLETED'
  AND ct.delete_nbr = 0
  AND tr.delete_nbr = 0;
