-- =============================================================================
-- Purpose : Rollback ux.activityTrackingColors from tenant_attr
-- Tenant  : KP only
-- Notes   : Idempotent; removes block only if it exists
-- Jira    : RES-54
-- =============================================================================
DO $$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>'; -- replace with actual KP tenant_code
BEGIN
    UPDATE tenant.tenant
    SET tenant_attr = tenant_attr::jsonb
        #- '{ux,activityTrackingColors,arrowBgColor}'
        #- '{ux,activityTrackingColors,arrowColor}'
        #- '{ux,activityTrackingColors,selectedDateBgColor}'
        #- '{ux,activityTrackingColors,selectedDate}'
        #- '{ux,activityTrackingColors,nonSelectedDay}'
        #- '{ux,activityTrackingColors,dotColor}'
        #- '{ux,activityTrackingColors,pipeline}'
        #- '{ux,activityTrackingColors,buttonColor}'
        #- '{ux,activityTrackingColors,updateTime}'
        #- '{ux,activityTrackingColors,textColor}'
        #- '{ux,activityTrackingColors,addActivityButton}'
        #- '{ux,activityTrackingColors,bgColor}'
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0;

    RAISE NOTICE 'Rollback complete: Removed provided activityTrackingColors keys for tenant %', v_tenant_code;
END $$;
