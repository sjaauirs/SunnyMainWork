-- Rollback script for SRCP-3553.sql
DELETE FROM admin.script
WHERE script_code = 'src-2827d987b0d1427a8c74921f591e83f6';

DELETE FROM admin.event_handler_script
WHERE event_handler_code IN (
    SELECT event_handler_code
    FROM admin.event_handler_script
    WHERE script_id = (SELECT script_id FROM admin.script WHERE script_code = 'src-2827d987b0d1427a8c74921f591e83f6')
      AND event_type = 'PICK_A_PURSE'
      AND event_sub_type = 'NONE'
      AND tenant_code IN (SELECT tenant_code FROM tenant.tenant WHERE delete_nbr = 0)
);