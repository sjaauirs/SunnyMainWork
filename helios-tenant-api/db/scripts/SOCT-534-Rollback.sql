/*
===============================================================================
 Script   : Rollback - Remove or Restore tileLinear3Color and tileLinear4Color
 Author  : [Vinod Ullaganti]
 Date    : 2025-05-16
 Purpose  : This script reverts previously updated color values inside the 
            'tenant_attr' JSONB column for each tenant in the 'tenant.tenant' table.
            Specifically, it targets the keys:
              - tileLinear3Color
              - tileLinear4Color
            located under ux > taskTileColors.

 Table    : tenant.tenant
 Column   : tenant_attr (type: JSONB)
 Keys     : ux.taskTileColors.tileLinear3Color, ux.taskTileColors.tileLinear4Color

 Options  :
   🔁 OPTION 1 - Permanently remove the keys from tenant_attr
   🛠️ OPTION 2 - Restore their old values (if known)

 Caution  : Run only if you're sure the original update should be reversed.
            Affects all tenants where:
              - delete_nbr = 0 (active)
              - tenant_attr IS NOT NULL or empty
===============================================================================
*/

-- 🔁 OPTION 1: REMOVE tileLinear3Color and tileLinear4Color from tenant_attr
UPDATE tenant.tenant
SET tenant_attr = tenant_attr
    #- '{ux,taskTileColors,tileLinear4Color}'  -- Remove tileLinear4Color key
    #- '{ux,taskTileColors,tileLinear3Color}'  -- Remove tileLinear3Color key
WHERE tenant_attr IS NOT NULL
  AND tenant_attr <> '{}'::jsonb
  AND delete_nbr = 0;

-- 🛠️ OPTION 2: RESTORE old values (if known) - replace with actual previous color values
/*
UPDATE tenant.tenant
SET tenant_attr = jsonb_set(
    jsonb_set(
        tenant_attr,
        '{ux,taskTileColors,tileLinear3Color}', '"#CCCCCC"'::jsonb  -- Restore previous tileLinear3Color
    ),
    '{ux,taskTileColors,tileLinear4Color}', '"#DDDDDD"'::jsonb       -- Restore previous tileLinear4Color
)
WHERE tenant_attr IS NOT NULL
  AND tenant_attr <> '{}'::jsonb
  AND delete_nbr = 0;
*/


