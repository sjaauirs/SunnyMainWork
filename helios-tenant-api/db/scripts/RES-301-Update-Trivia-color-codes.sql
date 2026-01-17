DO
$$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>'; -- Replace with actual tenant code
    v_input_json JSONB := '[
        {
            "hapTriviaColours": {
                "triviaQuestionTextColor": "#181D27",
                "triviaOptionsTextColor": "#181D27",
                "selectedOptionTextColor": "#FFFFFF",
                "triviaOptionsBorderColor": "#181D27",
                "correctOptionBorderColor": "#0A855C",
                "wrongOptionBorderColor": "#D43211",
                "triviaOptionBgColor": "#FFFFFF",
                "triviaOptionDisableBgColor": "#FFFFFF",
                "triviaOptionDisableTextColor": "#181D27",
                "triviaOptionsDisableBorderColor": "#181D27",
                "wrongOptionBgColor": "#D43211",
                "correctOptionBgColor": "#0A855C",
                "triviaBgColorWebDesktop": "#FBF8F6",
                "triviaBgColorMobile": "#FFFFFF",
                "successTintColor": "#0A855C",
                "wrongTintColor": "#D43211",
                "triviaInfoHeadingTextColor": "#181D27",
                "triviaInfoDescriptionTextColor": "#181D27",
                "learningModalBgColor": "#FFFFFF",
                "nextButtonBgColor": "#FFFFFF",
                "nextButtonBorderColor": "#181D27",
                "nextButtonTextColor": "#181D27",
                "progressBarFillColor": "#FF7200",
                "progressBarBgColor": "#C9CACC"
            }   
        }
    ]'::JSONB;

    v_hap_json JSONB;
    v_hap_tenants TEXT[];
BEGIN
    -- Extract hapTriviaColours JSON
    v_hap_json := (v_input_json->0->'hapTriviaColours')::JSONB;

    -- Update HAP tenant and collect tenant code
    WITH updated AS (
        UPDATE tenant.tenant
        SET tenant_attr = jsonb_set(
            tenant_attr,
            '{ux,triviaColors}',   -- Path to update
            v_hap_json,
            false
        ),
        update_ts = NOW(),
        update_user = 'SYSTEM'
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0
        RETURNING tenant_code
    )
    SELECT array_agg(tenant_code) INTO v_hap_tenants FROM updated;

    -- Raise Notice
    IF v_hap_tenants IS NOT NULL THEN
        RAISE NOTICE 'Updated triviaColors for HAP tenant(s): %', v_hap_tenants;
    ELSE
        RAISE NOTICE 'No HAP tenant updated for tenant_code: %', v_tenant_code;
    END IF;

    RAISE NOTICE 'Trivia colors update process completed for HAP tenant only.';
END
$$;
