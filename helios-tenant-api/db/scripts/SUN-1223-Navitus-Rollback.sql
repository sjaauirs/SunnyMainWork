DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'NAVITUS-TENANT-CODE'
    ];
    v_tenant TEXT;
BEGIN
    FOREACH v_tenant IN ARRAY v_tenant_codes LOOP
        UPDATE tenant.tenant
        SET tenant_attr = tenant_attr
            -- Root-level flags added/overwritten by the forward script
            - 'displayMobileHeader'
            - 'hideNameInitials'
            - 'enableStoresSection'
            - 'displayBancorpCopyright'
            - 'isTermsAndConditionVisibleForOrderCard'
            - 'displayAlternateDatePicker'

            -- Nested properties from the forward script
            #- '{ux,mycardColors,walletBgColor}'
            #- '{tenantAttribute,ux,headerColors,headerBgColor}'
            #- '{ux,themeColors,headerBgColor}'
            #- '{headerColors,headerBgColor}'
            #- '{commonColors,textColor7}'
            #- '{commonColors,contentBgColor}'
            #- '{ux,themeColors,taskGradient1}'
            #- '{ux,themeColors,taskGradient2}'
            #- '{ux,taskTileColors,activeTabBgColor}'
            #- '{triviaColors,progressBarFillColor}'
            #- '{commonColors,borderColor2}'
            #- '{triviaColors,triviaOptionsTextColor}'
            #- '{tenantAttribute,TriviaMobileImage}',

            update_ts = NOW()
        WHERE tenant_code = v_tenant
          AND delete_nbr = 0;

        RAISE NOTICE '🔁 Rolled back attributes for Navitus tenant: %', v_tenant;
    END LOOP;

    RAISE NOTICE '🏁 Completed rollback of tenant attributes for all NAVITUS tenants.';
END $$;
