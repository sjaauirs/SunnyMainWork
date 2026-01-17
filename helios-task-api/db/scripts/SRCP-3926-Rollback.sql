-- This is a rollback script SRCP-3926_map_costco_action_to_external_task file
DELETE FROM task.task_external_mapping 
WHERE task_third_party_code = 'earn_65_at_costco'
AND task_external_code = 'earn_65_at_cost'
AND create_user = 'SYSTEM'
AND delete_nbr = 0;
