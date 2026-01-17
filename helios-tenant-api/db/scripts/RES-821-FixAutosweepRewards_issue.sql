-- ============================================================================  
-- 🚀 Script    : RES-821  
-- 📌 Purpose   : Update autosweepSweepstakesReward to true for KP Tenant
-- 🧑 Author    : Neel
-- 📅 Date      : 2025-11-021
-- 🧾 Jira      : RES-821
-- ⚠️ Inputs    : KP-Tenant-Code  
-- ============================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'KP-TENANT-CODE'
    ];
    v_tenant_code TEXT;
    v_rowcount INT := 0;
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP                
        -- Update the tenant attribute JSON to include the "autosweepSweepstakesReward" flag.
        -- This ensures that all valid tenants have the "autosweepSweepstakesReward" property set to default value as false.
        UPDATE tenant.tenant
        SET tenant_attr = jsonb_set(
            tenant_attr,
            '{autosweepSweepstakesReward}',
            'true'::jsonb,
            true
        )
        WHERE delete_nbr = 0
            AND tenant_attr IS NOT NULL
            AND tenant_attr <> '{}'::jsonb            
            AND tenant_code = v_tenant_code
            ;

        GET DIAGNOSTICS v_rowcount = ROW_COUNT;
        RAISE NOTICE '✅ Tenant % → % rows updated', v_tenant_code, v_rowcount;
    END LOOP;
END $$;