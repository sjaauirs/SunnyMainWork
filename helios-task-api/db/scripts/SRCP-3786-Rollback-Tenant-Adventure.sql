-- Rollback script task.tenant_adventure

DELETE FROM task.tenant_adventure 
WHERE tenant_code = 'ten-ecada21e57154928a2bb959e8365b8b4'
AND delete_nbr =0