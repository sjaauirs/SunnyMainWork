-- ============================================================================
-- 🚀 Script    : Rollback - Restore Previous UX Colors in tenant_attr
-- 📌 Purpose   : Restores:
--                  - ux.cardActivationBannerColors.bannerBackgroundColorOnActivation = "148D79"
--                  - ux.cardActivationBannerColors.bannerTextColorOnActivation = "#326F91"
--                  - ux.shopColors.storeCardNameLabelColor = "#FFFFFF"
-- 👨‍💻 Author    : Riaz
-- 📅 Date      : 2025-12-12
-- 🧾 Jira      : SUN-1213-ROLLBACK
-- ⚠️ Inputs    : v_tenant_codes[]
-- 📝 Notes     :
--    - Idempotent & safe to run multiple times.
--    - Initializes missing JSON paths automatically.
-- ============================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'NAVITUS-TENANT-CODE'
    ];
    v_tenant TEXT;
BEGIN
    FOREACH v_tenant IN ARRAY v_tenant_codes LOOP

        UPDATE tenant.tenant t
        SET tenant_attr = (
            jsonb_set(
                jsonb_set(
                    jsonb_set(
                        jsonb_set(
                            COALESCE(t.tenant_attr::jsonb, '{}'::jsonb),
                            '{ux,cardActivationBannerColors,bannerBackgroundColorOnActivation}',
                            '"148D79"'::jsonb,
                            true
                        ),
                        '{ux,cardActivationBannerColors,bannerTextColorOnActivation}',
                        '"#326F91"'::jsonb,
                        true
                    ),
                    '{ux,shopColors,storeCardNameLabelColor}',
                    '"#FFFFFF"'::jsonb,
                    true
                ),
                '{}', '{}'::jsonb, true 
            )
        ) || jsonb_build_object('rollbackByScript', 'UX_CARD_BANNER_SHOPCOLOR_ROLLBACK_2025_12_12'),
        update_ts = NOW()
        WHERE t.tenant_code = v_tenant
          AND t.delete_nbr = 0;

        RAISE NOTICE '↩️ Rolled back cardActivationBannerColors & shopColors for tenant: %', v_tenant;
    END LOOP;

    RAISE NOTICE '🏁 Rollback completed for all tenants.';
END $$;
