ALTER TABLE wallet.transaction_detail
ADD COLUMN IF NOT EXISTS reward_description varchar(255) NULL;


-- use this query to update reward_description for existing records based on tenant code
UPDATE wallet.transaction_detail AS td
SET reward_description = td2.task_header
FROM task.task_reward AS tr
JOIN task.task_detail AS td2 ON tr.task_id = td2.task_id
JOIN task.consumer_task AS ct ON ct.task_id = td2.task_id 
                              AND tr.tenant_code = ct.tenant_code
							  AND tr.tenant_code = td2.tenant_code
WHERE td.task_reward_code = tr.task_reward_code
AND ct.consumer_code = td.consumer_code
AND tr.tenant_code = 'ten-ecada21e57154928a2bb959e8365b8b4'
AND td.transaction_detail_type = 'REWARD'
AND td.reward_description is null;