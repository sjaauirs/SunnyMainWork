-- =============================================================================
-- Purpose : Rollback ux.commonColors.textColor and buttonDisableColor
-- Tenant  : KP only
-- Notes   : Idempotent; removes keys only if present
-- Jira    : RES-54
-- =============================================================================
DO
$$
DECLARE
   v_tenant_code TEXT := '<KP-TENANT-CODE>'; -- replace with actual KP tenant_code
BEGIN
   -- Remove textColor
   UPDATE tenant.tenant
   SET tenant_attr = tenant_attr::jsonb #- '{ux,commonColors,textColor}'
   WHERE tenant_code = v_tenant_code;

   -- Remove buttonDisableColor
   UPDATE tenant.tenant
   SET tenant_attr = tenant_attr::jsonb #- '{ux,commonColors,buttonDisableColor}'
   WHERE tenant_code = v_tenant_code;
END;
$$;

