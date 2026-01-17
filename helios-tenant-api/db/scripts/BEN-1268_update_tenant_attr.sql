-- ============================================================================
-- 🚀 Script    : Add/Update UX Color Configurations in tenant_attr JSONB
-- 📌 Purpose   : Ensures tenant_attr contains or updates the following keys:
--                   - displayMobileHeader (true)
--                   - ux.forYouColors.forYouCardsHeadingBgColor (#0B0C0E)
--                   - ux.forYouColors.forYouCardsBgColor (#FFFFFF)
--                   - ux.commonColors.errorBorderColor (#D43211)
--                If a key exists → value is updated.
--                If missing → key is inserted.
-- 🧑 Author    : Rakesh Pernati
-- 📅 Date      : 2025-12-03
-- 🧾 Jira      : BEN-1268
-- ⚠️ Inputs    : v_tenant_codes (List of tenant identifiers)
-- 📤 Output    : Performs insert-or-update (upsert) on JSONB keys under tenant_attr
-- 🔗 Script URL: <Optional documentation or Confluence link>
-- 📝 Notes     : 
--                • Runs idempotently
--                • Safely constructs missing JSON paths
--                • Ensures consistent UX color configuration across tenants
-- ============================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
       '<NAVITUS-TENANT-CODE>',
       '<NAVITUS-TENANT-CODE>'
    ];

    v_tenant_code TEXT;
    v_old_attr JSONB;
    v_new_attr JSONB;
    v_updated BOOLEAN;

BEGIN
    RAISE NOTICE '===============================================';
    RAISE NOTICE '🚀 Starting UX Color Update Script...';
    RAISE NOTICE '===============================================';

    FOREACH v_tenant_code IN ARRAY v_tenant_codes LOOP
        RAISE NOTICE '➡️ Processing Tenant: %', v_tenant_code;

        v_updated := false;

        ----------------------------------------------------------------------
        --  Fetch existing tenant_attr
        ----------------------------------------------------------------------
        SELECT tenant_attr
          INTO v_old_attr
          FROM tenant.tenant
         WHERE tenant_code = v_tenant_code
           AND delete_nbr = 0;

        IF NOT FOUND THEN
            RAISE WARNING '⚠️ Tenant not found → skipping tenant: %', v_tenant_code;
            CONTINUE;
        END IF;

        IF v_old_attr IS NULL OR v_old_attr::text = '{}' THEN
            RAISE WARNING '⚠️ tenant_attr is NULL/empty → initializing new JSON for tenant: %', v_tenant_code;
            v_old_attr := '{}'::jsonb;
        END IF;

        v_new_attr := v_old_attr;

        ----------------------------------------------------------------------
        -- 1️⃣ displayMobileHeader
        ----------------------------------------------------------------------
        v_new_attr := jsonb_set(
                        v_new_attr,
                        '{displayMobileHeader}',
                        to_jsonb(true),
                        true
                      );
        v_updated := true;
        RAISE NOTICE '✔ displayMobileHeader updated for tenant %', v_tenant_code;


        ----------------------------------------------------------------------
        -- 2️⃣ forYouCardsHeadingBgColor
        ----------------------------------------------------------------------
        v_new_attr := jsonb_set(
                        v_new_attr,
                        '{ux,forYouColors,forYouCardsHeadingBgColor}',
                        to_jsonb('#0B0C0E'::text),
                        true
                      );
        v_updated := true;
        RAISE NOTICE '✔ forYouCardsHeadingBgColor updated for tenant %', v_tenant_code;
		
		 ----------------------------------------------------------------------
        -- 2️⃣ forYouCardsBgColor
        ----------------------------------------------------------------------
        v_new_attr := jsonb_set(
                        v_new_attr,
                        '{ux,forYouColors,forYouCardsBgColor}',
                        to_jsonb('#FFFFFF'::text),
                        true
                      );
        v_updated := true;
        RAISE NOTICE '✔ forYouCardsBgColor updated for tenant %', v_tenant_code;


        ----------------------------------------------------------------------
        -- 3️⃣ errorBorderColor
        ----------------------------------------------------------------------
        v_new_attr := jsonb_set(
                        v_new_attr,
                        '{ux,commonColors,errorBorderColor}',
                        to_jsonb('#D43211'::text),
                        true
                      );
        v_updated := true;
        RAISE NOTICE '✔ errorBorderColor updated for tenant %', v_tenant_code;
		
		


        ----------------------------------------------------------------------
        -- 🔄 UPDATE ONLY IF CHANGED
        ----------------------------------------------------------------------
        IF v_updated THEN
            UPDATE tenant.tenant
               SET tenant_attr = v_new_attr,
                   update_ts = NOW(),
                   update_user = 'SYSTEM'
             WHERE tenant_code = v_tenant_code
               AND delete_nbr = 0;

            RAISE NOTICE '✅ tenant_attr updated successfully for tenant %', v_tenant_code;
        ELSE
            RAISE NOTICE 'ℹ No changes required for tenant %', v_tenant_code;
        END IF;

        RAISE NOTICE '------------------------------------------------';

    END LOOP;

    RAISE NOTICE '🎉 Script Completed Successfully for All Tenants!';
    RAISE NOTICE '===============================================';

EXCEPTION
    WHEN OTHERS THEN
        RAISE WARNING '❌ ERROR OCCURRED: %', SQLERRM;
        RAISE;
END $$;
