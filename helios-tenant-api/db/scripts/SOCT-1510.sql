DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY['tenant_code_1', 'tenant_code_2'];  -- Replace with actual tenant codes
    v_code TEXT;
    v_json JSONB;
BEGIN
    -- Part 1: Update tenant codes in the array to set shouldFreezeCardOnTermination to true
    FOREACH v_code IN ARRAY v_tenant_codes
    LOOP
        SELECT tenant_option_json INTO v_json
        FROM tenant.tenant
        WHERE tenant_code = v_code;

        IF v_json ? 'benefitsOptions' THEN
            v_json := jsonb_set(
                jsonb_set(
                    v_json,
                    '{benefitsOptions,shouldFreezeCardOnTermination}',
                    'true'::jsonb,
                    true
                ),
                '{benefitsOptions,validCardActiveDays}',
                '30'::jsonb,
                true
            );

            UPDATE tenant.tenant
            SET tenant_option_json = v_json
            WHERE tenant_code = v_code;

            RAISE NOTICE 'Updated TRUE for %', v_code;
        ELSE
            RAISE NOTICE 'Skipped (benefitsOptions not found) for %', v_code;
        END IF;
    END LOOP;

    -- Part 2: Set shouldFreezeCardOnTermination to false for tenants NOT in the array
    FOR v_code IN
        SELECT tenant_code FROM tenant.tenant WHERE tenant_code <> ALL(v_tenant_codes)
    LOOP
        SELECT tenant_option_json INTO v_json
        FROM tenant.tenant
        WHERE tenant_code = v_code;

        IF v_json ? 'benefitsOptions' THEN
            v_json := jsonb_set(
                v_json,
                '{benefitsOptions,shouldFreezeCardOnTermination}',
                'false'::jsonb,
                true
            );

            UPDATE tenant.tenant
            SET tenant_option_json = v_json
            WHERE tenant_code = v_code;

            RAISE NOTICE 'Updated FALSE for %', v_code;
        ELSE
            RAISE NOTICE 'Skipped (benefitsOptions not found) for %', v_code;
        END IF;
    END LOOP;
END $$;