-- =====================================================================
-- Script  : Add userDisplayNameField to tenant_attr
-- Jira    : BEN-427
-- Purpose : We need to display user name as First Name in UI for HAP tenant, but as per existing code
--           we are display name as Auth0 name
-- =====================================================================
DO $$
DECLARE
  v_tenant_code  text := '<HAP-Tenant-code>'; -- Replease this tenant code with HAP tenant code
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
      tenant_attr,
      '{userDisplayNameField}',
      '"FIRST_NAME"'::jsonb, 
      true
  )
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0
    AND tenant_attr IS NOT NULL
    AND tenant_attr <> '{}'::jsonb
    AND NOT tenant_attr ? 'userDisplayNameField';
END $$;
