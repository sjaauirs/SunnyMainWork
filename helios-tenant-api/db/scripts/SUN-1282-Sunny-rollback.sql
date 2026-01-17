-- 🚀 Script    : Rollback marketing banner keys for Sunny
-- 📌 Purpose   : Rollback marketing banner keys for Sunny
-- 👨‍💻 Author    : Charan
-- 📅 Date      : 2025-12-31
-- 🧾 Jira      : SUN-1282
-- ⚠️ Inputs    : SUNNY-TENANT-CODE
-- 📤 Output    : Updates tenant_attr JSON structure
-- 📝 Notes     :
--    - Removes marketing banner keys from tenant_attr.
--    - Idempotent: safe to run multiple times.
-- ============================================================================



-- 🔁 ROLLBACK SCRIPT: Remove ONLY the newly added keys (no clobber of cardActivationBannerColors)
DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'SUNNY-TENANT-CODE'
    ];
    v_tenant_code TEXT;
    v_now         TIMESTAMP := NOW();
    v_user        TEXT := 'SYSTEM';
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        UPDATE tenant.tenant t
        SET tenant_attr =
            -- 2) Remove top-level keys
            jsonb_set(
                (
                    COALESCE(t.tenant_attr::jsonb, '{}'::jsonb)
                    - 'marketingBannerEnabled'
                    - 'marketingBannerText'
                ),
                -- 1) Remove only the two nested keys inside ux.cardActivationBannerColors
                '{ux,cardActivationBannerColors}',
                (
                    COALESCE(t.tenant_attr::jsonb #> '{ux,cardActivationBannerColors}', '{}'::jsonb)
                    - 'marketingBannerBackgroundColor'
                    - 'marketingBannerTextColor'
                ),
                true
            ),
            update_user = v_user,
            update_ts   = v_now
        WHERE t.tenant_code = v_tenant_code
          AND t.delete_nbr = 0;

        RAISE NOTICE '↩️ Rolled back ONLY marketing banner keys for tenant %', v_tenant_code;
    END LOOP;
END $$;