DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY['tenant_code_1', 'tenant_code_2'];  -- Replace with actual tenant codes
    v_code TEXT;
    v_json JSONB;
BEGIN
    -- Part 1: Rollback for tenant codes in the array
    FOREACH v_code IN ARRAY v_tenant_codes
    LOOP
        SELECT tenant_option_json INTO v_json
        FROM tenant.tenant
        WHERE tenant_code = v_code;

        IF v_json ? 'benefitsOptions' THEN
            v_json := v_json
                - 'benefitsOptions' || jsonb_build_object(
                    'benefitsOptions',
                    (v_json->'benefitsOptions') - 'shouldFreezeCardOnTermination' - 'validCardActiveDays'
                );

            UPDATE tenant.tenant
            SET tenant_option_json = v_json
            WHERE tenant_code = v_code;

            RAISE NOTICE 'Rolled back (true + days) for %', v_code;
        ELSE
            RAISE NOTICE 'Skipped rollback (benefitsOptions not found) for %', v_code;
        END IF;
    END LOOP;

    -- Part 2: Rollback for tenants NOT in the array (remove the false flag)
    FOR v_code IN
        SELECT tenant_code FROM tenant.tenant WHERE tenant_code <> ALL(v_tenant_codes)
    LOOP
        SELECT tenant_option_json INTO v_json
        FROM tenant.tenant
        WHERE tenant_code = v_code;

        IF v_json ? 'benefitsOptions' THEN
            v_json := v_json
                - 'benefitsOptions' || jsonb_build_object(
                    'benefitsOptions',
                    (v_json->'benefitsOptions') - 'shouldFreezeCardOnTermination'
                );

            UPDATE tenant.tenant
            SET tenant_option_json = v_json
            WHERE tenant_code = v_code;

            RAISE NOTICE 'Rolled back (false flag) for %', v_code;
        ELSE
            RAISE NOTICE 'Skipped rollback (benefitsOptions not found) for %', v_code;
        END IF;
    END LOOP;
END $$;