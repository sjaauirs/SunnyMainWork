-- =================================================================================================================================
-- 🚀 Script    : Update CARD ISSUE FLOW TYPE in tenant_option_json
-- 📌 Purpose   : To update "flowType": "TASK_COMPLETION_CHECK" → "AGREEMENTS_VERIFIED"
--                within benefitsOptions.cardIssueFlowType array in tenant.tenant table.
-- 🧑 Author    : Preeti
-- 📅 Date      : 2025-10-22
-- 🧾 Jira      : SUN-801
-- ⚠️ Inputs    : TENANT_CODE
-- 📤 Output    : Updated tenant_option_json for given tenant(s)
-- 🔗 Script URL: Internal configuration update (QA only)
-- 📝 Notes     : 
--   - JSON key path: benefitsOptions → cardIssueFlowType
--   - Safe to re-run (idempotent)
-- =================================================================================================================================


DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'HAP-TENANT-CODE'  -- add more tenant codes as needed
    ];
    v_tenant_code TEXT;
    v_updated_count INTEGER := 0;
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        RAISE NOTICE 'Processing tenant: %', v_tenant_code;

        UPDATE tenant.tenant
        SET tenant_option_json = jsonb_set(
                                    tenant_option_json,
                                    '{benefitsOptions,cardIssueFlowType}',
                                    (
                                        SELECT jsonb_agg(
                                            CASE
                                                WHEN elem->>'flowType' = 'TASK_COMPLETION_CHECK' THEN
                                                    jsonb_set(elem, '{flowType}', to_jsonb('AGREEMENTS_VERIFIED'::text))
                                                ELSE elem
                                            END
                                        )
                                        FROM jsonb_array_elements(
                                            tenant_option_json#>'{benefitsOptions,cardIssueFlowType}'
                                        ) AS elem
                                    )
                                ),
            update_ts = NOW(),
            update_user = 'SYSTEM'
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        GET DIAGNOSTICS v_updated_count = ROW_COUNT;

        IF v_updated_count > 0 THEN
            RAISE NOTICE '✅ Updated flowType for tenant %', v_tenant_code;
        ELSE
            RAISE NOTICE '⚠️ No matching record found or already updated for tenant %', v_tenant_code;
        END IF;
    END LOOP;

    RAISE NOTICE 'Update process completed for all tenants.';
END
$$;