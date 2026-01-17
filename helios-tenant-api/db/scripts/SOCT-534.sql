/*
===============================================================================
 Script  : Update Task Tile Colors in tenant.tenant.tenant_attr (JSONB column)
 Author  : [Vinod Ullaganti]
 Date    : 2025-05-16
 Purpose : This script updates the 'tenant_attr' JSONB column in the 
           'tenant.tenant' table for all active tenants. It sets the following 
           UI color values under the nested JSON path ux > taskTileColors:
           
           - tileLinear3Color → "#2E8807"
           - tileLinear4Color → "#48D50B"
===============================================================================

 Preconditions:
   - Table         : tenant.tenant
   - Column        : tenant_attr (JSONB)
   - Update only rows where:
       - tenant_attr IS NOT NULL
       - tenant_attr IS NOT empty
       - delete_nbr = 0 (i.e., tenant is active)
===============================================================================
*/

DO $$
DECLARE
    affected_rows INT;
BEGIN
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
        jsonb_set(
            tenant_attr,
            '{ux,taskTileColors,tileLinear3Color}', '"#2E8807"'::jsonb
        ),
        '{ux,taskTileColors,tileLinear4Color}', '"#48D50B"'::jsonb
    )
    WHERE tenant_attr IS NOT NULL
      AND tenant_attr <> '{}'::jsonb
      AND delete_nbr = 0;

    GET DIAGNOSTICS affected_rows = ROW_COUNT;

    IF affected_rows > 0 THEN
        RAISE NOTICE '✅ Success: Updated % row(s) with new tileLinear colors.', affected_rows;
    ELSE
        RAISE NOTICE '⚠️ No rows updated. Please check if there are active tenants with valid tenant_attr.';
    END IF;
END $$;
