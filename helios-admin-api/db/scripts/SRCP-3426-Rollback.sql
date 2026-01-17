-- Rollback script for SRCP-3426.sql
DELETE FROM admin.script
WHERE script_code = 'src-2753bd3756f146d28a9a895ab78bca9d';

DELETE FROM admin.event_handler_script
WHERE event_handler_code IN (
    SELECT event_handler_code
    FROM admin.event_handler_script
    WHERE script_id = (SELECT script_id FROM admin.script WHERE script_code = 'src-2753bd3756f146d28a9a895ab78bca9d')
      AND event_type = 'TASK_TRIGGER'
      AND event_sub_type = 'HEALTH_TASK_PROGRESS'
      AND tenant_code IN (SELECT tenant_code FROM tenant.tenant WHERE delete_nbr = 0)
);