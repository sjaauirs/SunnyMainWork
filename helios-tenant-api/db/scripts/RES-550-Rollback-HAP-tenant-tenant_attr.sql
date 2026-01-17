-- 🚀 Script    : Rollback colour codes from tenant_attr
-- 📌 Purpose   : To rollback the colour codes added for HAP-TENANT-CODE in tenant_attr 
-- 🧑 Author    : Siva Krishna
-- 📅 Date      : 2025-09-22
-- 🧾 Jira      : RES-550
-- ⚠️ Inputs    : HAP-TENANT-CODE
-- 📤 Output    : It will remove only the specific colour code key-value pairs 
--                (borderColor, actionsBgColor, buttonBgColorDesktop, buttonBgColorMobile) 
--                from tenant_attr, keeping other keys intact


DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>';  -- Replace with your tenant_code
    v_taskTileColors JSONB := '{
        "completedTileBgColor": "#868C92",
        "inProgressBgColor": "#0B3B60"
    }';
    v_walletColors JSONB := '{
        "strokeEarnedColor": "#181D27",
		"strokeSegmentColor": "#D3D6DC"
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
        RAISE NOTICE 'Tenant with code % and delete_nbr = 0 not found.', v_tenant_code;
        RETURN;
    END IF;

    -- Step 2: Ensure tenant_attr exists
    IF v_tenant_attr IS NULL THEN
        RAISE NOTICE 'tenant_attr is null, nothing to rollback';
        RETURN;
    END IF;

    -- Step 3: Rollback only the specific color keys in "taskTileColors"
    IF v_tenant_attr->'ux' ? 'taskTileColors' THEN
        v_tenant_attr := jsonb_set(
            v_tenant_attr,
            '{ux,taskTileColors}',
            (v_tenant_attr->'ux'->'taskTileColors') 
              - 'borderColor' 
              - 'actionsBgColor' 
              - 'completeTextFgColor' 
              - 'ctaButtonBgColor'
        );
        RAISE NOTICE 'Rolled back specific keys in taskTileColors';
    END IF;

    -- Step 4: Rollback to previous "taskTileColors"
    v_tenant_attr := jsonb_set(
        v_tenant_attr,
        '{ux,taskTileColors}',
        COALESCE(v_tenant_attr->'ux'->'taskTileColors', '{}'::jsonb) || v_taskTileColors,
        true
    );
    RAISE NOTICE 'taskTileColors updated successfully';

    -- Step 5: Rollback to previous "walletColors"
    v_tenant_attr := jsonb_set(
        v_tenant_attr,
        '{ux,walletColors}',
        COALESCE(v_tenant_attr->'ux'->'walletColors', '{}'::jsonb) || v_walletColors,
        true
    );
    RAISE NOTICE 'walletColors updated successfully';

    -- Step 6: Persist changes
    UPDATE tenant.tenant
    SET tenant_attr = v_tenant_attr
    WHERE tenant_id = v_tenant_id;

    RAISE NOTICE 'Tenant color codes rollback completed';
END $$;
