-- ============================================================================
-- 🚀 Script    : Add marketing banner keys for HAP
-- 📌 Purpose   : Add marketing banner keys for HAP
-- 👨‍💻 Author    : Charan
-- 📅 Date      : 2025-12-31
-- 🧾 Jira      : SUN-1283
-- ⚠️ Inputs    : HAP-TENANT-CODE
-- 📤 Output    : Updates tenant_attr JSON structure
-- 📝 Notes     :
--    - Adds or overwrites marketing banner keys in tenant_attr.
--    - Idempotent: safe to run multiple times.
-- ============================================================================


DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'HAP-TENANT-CODE'
    ];
    v_tenant_code TEXT;
    v_now         TIMESTAMP := NOW();
    v_user        TEXT := 'SYSTEM';
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        UPDATE tenant.tenant t
        SET tenant_attr =
            -- 3) Add top-level marketingBannerText
            jsonb_set(
                -- 2) Add top-level marketingBannerEnabled
                jsonb_set(
                    -- 1) Add keys inside ux.cardActivationBannerColors
                    jsonb_set(
                        COALESCE(t.tenant_attr::jsonb, '{}'::jsonb),
                        '{ux,cardActivationBannerColors}',
                        COALESCE(t.tenant_attr::jsonb #> '{ux,cardActivationBannerColors}', '{}'::jsonb)
                        || jsonb_build_object(
                            'marketingBannerBackgroundColor', '#E3E4E5',
                            'marketingBannerTextColor', '#181D27'
                        ),
                        true
                    ),
                    '{marketingBannerEnabled}',
                    to_jsonb(true),
                    true
                ),
                '{marketingBannerText}',
                jsonb_build_object(
                    'en-US', 'Use your card at Kroger, Meijer, Walgreens, and many other stores. Some Walmart and Target locations may not be able to process card payments right now.',
                    'es',    'Utilice su tarjeta en Kroger, Meijer, Walgreens y muchos otros establecimientos. Es posible que algunas sucursales de Walmart y Target no puedan procesar pagos con tarjeta en este momento.'
                ),
                true
            ),
            update_user = v_user,
            update_ts   = v_now
        WHERE t.tenant_code = v_tenant_code
          AND t.delete_nbr = 0;

        RAISE NOTICE '✅ Added marketing banner keys for tenant %', v_tenant_code;
    END LOOP;
END $$;
