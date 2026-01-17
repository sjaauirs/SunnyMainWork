
-- ============================================================================
-- 🚀 Script    : Add UX shopColors styling to Tenant Attribute for NAVITUS
-- 📌 Purpose   : Adds or replaces the "shopColors" object inside "ux" in tenant_attribute JSONB
-- 🧑 Author    : Bhojesh 
-- 📅 Date      : 2025-11-19
-- 🧾 Jira      : BEN-1278
-- ⚠️ Inputs    : <NAVITUS-TENANT-CODE>
-- 📤 Output    : Updated tenant_attribute JSONB with new shopColors styling
-- 🔗 Script URL: <NA>
-- 📝 Notes     : This script assumes the tenant_attribute column is of type JSONB.
--               If "shopColors" already exists, it will be overwritten.

-- ============================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'ten-a468348402cd438ea9a1005ae2faedb6'
    ];

    v_shop_colors JSONB;
    v_tenant_code TEXT;
BEGIN
    -- Build large JSON object using jsonb_object() to avoid 100-argument limit
    v_shop_colors := jsonb_object(ARRAY[
        'errorColor', '#D43211',
        'purseNameColor', '#0B0C0E',
        'tabBorderColor', 'transparent',
        'purseLabelColor', '#0B0C0E',
        'removeIconColor', '#3D3F42',
        'searchIconColor', '#0B0C0E',
        'activeTabBgColor', '#21495F',
        'purseAmountColor', '#0B0C0E',
        'storeCardBgColor', '#FFFFFF',
        'disabledTextColor', '#5F6062',
        'inactiveTabBgColor', '#FFFFFF',
        'activeTabLabelColor', '#FFFFFF',
        'disabledBorderColor', '#CBCCCD',
        'purseBackgroundColor', '#FFFFFF',
        'purseCardBorderColor', '#CBCCCD',
        'viewAllButtonBgColor', '#326F91',
        'zipCodeButtonBgColor', '#326F91',
        'inactiveTabLabelColor', '#0B0C0E',
        'purseDescriptionColor', '#0B0C0E',
        'scanItemButtonBgColor', '#21495F',
        'searchInputLabelColor', '#0B0C0E',
        'purseShopButtonBgColor', '#FFFFFF',
        'searchPlaceholderColor', '#5F6062',
        'storeCardButtonBgColor', '#326F91',
        'storeCardNameLabelColor', '#FFFFFF',
        'storeCardOpenLabelColor', '#148D79',
        'viewAllButtonLabelColor', '#FFFFFF',
        'zipCodeButtonLabelColor', '#FFFFFF',
        'purseIconBackgroundColor', '#FFFFFF',
        'purseSelectedBorderColor', '#F7F7F7',
        'scanItemButtonLabelColor', '#0B0C0E',
        'storeCardMilesLabelColor', '#0B0C0E',
        'viewAllButtonBorderColor', 'transparent',
        'zipCodeButtonBorderColor', 'transparent',
        'disabledButtonBorderColor', 'transparent',
        'purseShopButtonLabelColor', '#0B0C0E',
        'scanItemButtonBorderColor', 'transparent',
        'storeCardButtonLabelColor', '#FFFFFF',
        'storeCardClosedLabelColor', '#535353',
        'getDirectionsButtonBgColor', '#0B0C0E',
        'purseShopButtonBorderColor', 'transparent',
        'shopLocationsZipLabelColor', '#326F91',
        'storeCardAddressLabelColor', '#535353',
        'storeCardButtonBorderColor', 'transparent',
        'disabledButtonBackgroundColor', '#E3E5E8',
        'getDirectionsButtonLabelColor', '#0B0C0E',
        'getDirectionsButtonBorderColor', 'transparent',
        'searchRecommendationLabelColor', '#0B0C0E',
        'shopLocationsHeadingLabelColor', '#0B0C0E',
        'storeCardCloseDescriptionColor', '#535353',
        'shopLocationsDescriptionLabelColor', '#3D3F42',
		'shopPaginationActiveColor','#326F91',
		'shopPaginationInactiveColor','#CBCCCD'
    ]);

    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        UPDATE tenant.tenant
        SET tenant_attr = jsonb_set(
            tenant_attr,
            '{ux,shopColors}',
            v_shop_colors,
            true
        )
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        RAISE NOTICE '[INFO] Updated shopColors for tenant: %', v_tenant_code;
    END LOOP;

END $$;