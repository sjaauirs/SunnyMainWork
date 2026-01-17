-- Rollback script for admin.event_handler_script
DELETE FROM admin.event_handler_script
WHERE script_id = (
    SELECT script_id 
    FROM admin.script 
    WHERE script_code = 'src-266c482017564f15960c11dbea1ae40b'
)
AND event_type = 'CONSUMER_TASK'
AND event_sub_type = 'CONSUMER_TASK_UPDATE';

-- Rollback script for admin.script
DELETE FROM admin.script 
WHERE script_code = 'src-266c482017564f15960c11dbea1ae40b';
