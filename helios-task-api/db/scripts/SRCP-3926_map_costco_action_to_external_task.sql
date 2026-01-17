-- This script maps the Costco action to its corresponding external action in the task_external_mapping table.
INSERT INTO task.task_external_mapping (tenant_code, task_third_party_code, 
    task_external_code, create_ts, update_ts, create_user, update_user, delete_nbr)
SELECT t.tenant_code, 'earn_65_at_costco', 'earn_65_at_cost', 
    CURRENT_TIMESTAMP AT TIME ZONE 'UTC', NULL, 'SYSTEM', '', 0
FROM tenant.tenant t
LEFT JOIN task.task_external_mapping tem
ON tem.tenant_code = t.tenant_code 
AND tem.task_third_party_code = 'earn_65_at_costco'
AND tem.task_external_code = 'earn_65_at_cost'
WHERE t.delete_nbr = 0
AND tem.tenant_code IS NULL; -- Ensures only tenants not already mapped are inserted
