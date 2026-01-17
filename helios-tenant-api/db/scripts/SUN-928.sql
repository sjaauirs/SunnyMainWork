-- ============================================================================  
-- 🚀 Script    : SUN-928  
-- 📌 Purpose   : Update enableLiveChatbot to true for KP Tenant
-- 🧑 Author    : Neel
-- 📅 Date      : 2025-11-03  
-- 🧾 Jira      : SUN-928  
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
        UPDATE tenant.tenant
        SET tenant_attr = jsonb_set(
            tenant_attr,
            '{liveChatbotInfo,enableLiveChatbot}',
            'true'::jsonb,
            false
        ),
        update_ts = NOW(),
        update_user = 'SYSTEM'
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        GET DIAGNOSTICS v_rowcount = ROW_COUNT;
        RAISE NOTICE '✅ Tenant % → % rows updated', v_tenant_code, v_rowcount;
    END LOOP;
END $$;