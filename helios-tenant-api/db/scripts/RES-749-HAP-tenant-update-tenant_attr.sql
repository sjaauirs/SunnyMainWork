-- 🚀 Script    : Add colour codes to tenant_attr
-- 📌 Purpose   : For HAP-TENANT-CODE need to update tenant_attr with additional colour codes
-- 🧑 Author    : Siva Krishna
-- 📅 Date      : 2025-09-22
-- 🧾 Jira      : RES-749
-- ⚠️ Inputs    : HAP-TENANT-CODE 
-- 📤 Output    : It will updates tenant_attr with input color codes
-- 🔗 Script URL: NA
-- 📝 Notes     : NA
DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>';  -- Replace with input tenant_code
    v_taskTileColors JSONB := '{
         "checkBoxBorderColor": "#FE7200",
         "checkBoxBgColor":"#FE7200",
         "dropDownBorderColor": "#757980",
         "disableBorderColor": "#ABAEB2"
        }';
	v_calendarColors JSONB := '{
        "monthLabelColor": "#1C1C1C"	  
    }';
    
    v_tenant_id BIGINT;
    v_tenant_attr JSONB;
BEGIN
    -- Step 1: Fetch tenant
    SELECT tenant_id, tenant_attr
    INTO v_tenant_id, v_tenant_attr
    FROM tenant.tenant
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0
    LIMIT 1;
    
    IF NOT FOUND THEN
        RAISE NOTICE '[Warning]: Tenant with code % and delete_nbr = 0 not found.', v_tenant_code;
        RETURN;
    END IF;
    
    RAISE NOTICE '[Information]: Tenant found with tenant_id %', v_tenant_id;
    
    -- Step 2: Ensure tenant_attr JSONB exists
    IF v_tenant_attr IS NULL THEN
        v_tenant_attr := '{}'::jsonb;
        RAISE NOTICE '[Warning]: tenant_attr is null, initializing as empty JSONB';
    END IF;
    
    -- Step 3: Update "ux" node if not exists
    IF NOT v_tenant_attr ? 'ux' THEN
        v_tenant_attr := jsonb_set(v_tenant_attr, '{ux}', '{}'::jsonb);
        RAISE NOTICE '[Information]: "ux" node created';
    END IF;
    
    -- Step 4: Update "taskTileColors"
    v_tenant_attr := jsonb_set(
        v_tenant_attr,
        '{ux,taskTileColors}',
        COALESCE(v_tenant_attr->'ux'->'taskTileColors', '{}'::jsonb) || v_taskTileColors,
        true
    );
    RAISE NOTICE '[Information]: taskTileColors updated successfully';
	
	 -- Step 5: Update "calendarColors"
    v_tenant_attr := jsonb_set(
        v_tenant_attr,
        '{ux,calendarColors}',
        COALESCE(v_tenant_attr->'ux'->'calendarColors', '{}'::jsonb) || v_calendarColors,
        true
    );
    RAISE NOTICE '[Information]: calendarColors updated successfully';
    
    -- Step 6: Persist changes
    UPDATE tenant.tenant
    SET tenant_attr = v_tenant_attr
    WHERE tenant_id = v_tenant_id;
    
    RAISE NOTICE '[Information]: Tenant attributes updated successfully';
END $$;
