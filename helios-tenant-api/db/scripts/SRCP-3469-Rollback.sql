-- Rollback Tenant attribute changes:
-- Remove 'justInTimeFunding' and 'jitfTimeOffset' from tenant_attr

UPDATE tenant.tenant
SET tenant_attr = tenant_attr
	    - 'justInTimeFunding'  
            - 'jitfTimeOffset'  
WHERE delete_nbr = 0;

