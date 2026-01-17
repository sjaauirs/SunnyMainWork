-- This script updates the `tenant_attr` column in the 
-- `tenant.tenant` table by adding `taskTileColors`, `walletColors` and 
--  `entriesWalletColors` inside the existing `ux` object.
--  The script ensures `themeColors` remains unchanged.

UPDATE tenant.tenant
SET tenant_attr = jsonb_set(
    tenant_attr, 
    '{ux}', 
    tenant_attr->'ux' || '{
      "walletColors": {
        "availableSpendBgColor": "#217AB5",
        "strokeSegmentColor": "#D3D6DC",
        "strokeEarnedColor": "#57A635",
        "leftToEarnColor": "#133B71",
        "textFgColor": "#212121",
        "walletBgColor": "#FFFFFF",
        "redeemButtonColor": "#FFFFFF"
      },
      "entriesWalletColors": {
        "headerBgColor": "#217AB5",
        "headerTextColor": "#FFFFFF",
        "contentBgColor": "#D3D6DC",
        "contentTextColor": "#133B71"
      },
      "taskTileColors": {
        "tileLinear1Color": "#003B71",
        "tileLinear2Color": "#44B8F3",
        "inProgressBgColor": "#FEC941",
        "textColor": "#545454",
		"inProgressTextFgColor": "#2D7AB6"
      }
    }'::jsonb,
    true
)
WHERE tenant_attr IS NOT NULL
  AND tenant_attr <> '{}'::jsonb 
  AND tenant_attr ? 'ux'
  AND delete_nbr = 0;

