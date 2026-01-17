-- =================================================================================================================================
-- 🔁 ROLLBACK SCRIPT
-- 📌 Purpose   : Revert "flowType": "AGREEMENTS_VERIFIED" → "TASK_COMPLETION_CHECK"
-- 🧑 Author    : Preeti
-- 📅 Date      : 2025-10-22
-- =================================================================================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'HAP-TENANT-CODE'
    ];
    v_tenant_code TEXT;
    v_rollback_count INTEGER := 0;
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        UPDATE tenant.tenant
        SET tenant_option_json = jsonb_set(
                                    tenant_option_json,
                                    '{benefitsOptions,cardIssueFlowType}',
                                    (
                                        SELECT jsonb_agg(
                                            CASE
                                                WHEN elem->>'flowType' = 'AGREEMENTS_VERIFIED' THEN
                                                    jsonb_set(elem, '{flowType}', to_jsonb('TASK_COMPLETION_CHECK'::text))
                                                ELSE elem
                                            END
                                        )
                                        FROM jsonb_array_elements(
                                            tenant_option_json#>'{benefitsOptions,cardIssueFlowType}'
                                        ) AS elem
                                    )
                                ),
            update_ts = NOW(),
            update_user = 'ROLLBACK_USER'
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        GET DIAGNOSTICS v_rollback_count = ROW_COUNT;

        IF v_rollback_count > 0 THEN
            RAISE NOTICE '♻️ Rolled back flowType for tenant %', v_tenant_code;
        ELSE
            RAISE NOTICE '⚠️ No matching record found for tenant % during rollback.', v_tenant_code;
        END IF;
    END LOOP;

    RAISE NOTICE 'Rollback process completed for all tenants.';
END
$$;