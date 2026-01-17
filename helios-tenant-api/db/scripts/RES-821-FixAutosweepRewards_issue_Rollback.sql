-- ============================================================================  
-- 🚀 Script    : RES-821  
-- 📌 Purpose   : Revert autosweepSweepstakesReward for KP Tenant
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
        -- Rollback: Remove 'autosweepSweepstakesReward' flag from root level of tenant_attr
        UPDATE tenant.tenant
        SET tenant_attr = jsonb_set(
            tenant_attr,
            '{autosweepSweepstakesReward}',
            'false'::jsonb,
            true
        )
            AND tenant_attr <> '{}'::jsonb
            AND tenant_attr ? 'autosweepSweepstakesReward'
            AND delete_nbr = 0
            AND tenant_code = v_tenant_code;
            
        GET DIAGNOSTICS v_rowcount = ROW_COUNT;
        RAISE NOTICE '✅ Tenant % → % rows updated', v_tenant_code, v_rowcount;
    END LOOP;
END $$;