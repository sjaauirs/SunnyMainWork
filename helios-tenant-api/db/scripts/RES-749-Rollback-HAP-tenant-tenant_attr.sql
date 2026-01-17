-- 🚀 Rollback Script : Remove newly added color codes from tenant_attr
-- 📌 Purpose         : Rollback of RES-749 — removes 4 color keys from taskTileColors and 1 color key (monthLabelColor) from calendarColors for the given tenant
-- 🧑 Author          : Siva Krishna
-- 📅 Date            : 2025-09-22
-- 🧾 Jira            : RES-749
-- ⚠️ Inputs          : HAP-TENANT-CODE
-- 📤 Output          : Removes 4 color keys from ux.taskTileColors and monthLabelColor from ux.calendarColors
-- 🔗 Script URL      : NA
-- 📝 Notes           : Retains existing color keys if present

DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>';  -- Replace with actual tenant code
    v_tenant_id BIGINT;
    v_tenant_attr JSONB;
    v_task_tile_colors JSONB;
    v_calendar_colors JSONB;
BEGIN
    -- Step 1️: Fetch tenant record
    SELECT tenant_id, tenant_attr
    INTO v_tenant_id, v_tenant_attr
    FROM tenant.tenant
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0
    LIMIT 1;

    IF NOT FOUND THEN
        RAISE NOTICE '[Error] Tenant with code % and delete_nbr = 0 not found.', v_tenant_code;
        RETURN;
    END IF;

    RAISE NOTICE '[Information] Tenant found with tenant_id %', v_tenant_id;

    -- Step 2️: Ensure tenant_attr JSON exists
    IF v_tenant_attr IS NULL THEN
        RAISE NOTICE 'tenant_attr is NULL — rollback not required.';
        RETURN;
    END IF;

    -- Step 3️: Handle ux.taskTileColors removal
    IF (v_tenant_attr ? 'ux') AND (v_tenant_attr->'ux' ? 'taskTileColors') THEN
        v_task_tile_colors := v_tenant_attr->'ux'->'taskTileColors';

        -- Remove only the 4 added keys
        v_task_tile_colors := v_task_tile_colors
            - 'checkBoxBorderColor'
            - 'checkBoxBgColor'
            - 'dropDownBorderColor'
            - 'disableBorderColor';

        -- Update tenant_attr
        v_tenant_attr := jsonb_set(
            v_tenant_attr,
            '{ux,taskTileColors}',
            v_task_tile_colors,
            true
        );

        RAISE NOTICE '[Information] Removed taskTileColors keys: checkBoxBorderColor, checkBoxBgColor, dropDownBorderColor, disableBorderColor';
    ELSE
        RAISE NOTICE 'No taskTileColors found — skipping.';
    END IF;

    -- Step 4️: Handle ux.calendarColors removal
    IF (v_tenant_attr ? 'ux') AND (v_tenant_attr->'ux' ? 'calendarColors') THEN
        v_calendar_colors := v_tenant_attr->'ux'->'calendarColors';

        -- Remove monthLabelColor key
        v_calendar_colors := v_calendar_colors - 'monthLabelColor';

        -- Update tenant_attr
        v_tenant_attr := jsonb_set(
            v_tenant_attr,
            '{ux,calendarColors}',
            v_calendar_colors,
            true
        );

        RAISE NOTICE '[Information] Removed calendarColors key: monthLabelColor';
    ELSE
        RAISE NOTICE 'No calendarColors found — skipping.';
    END IF;

    -- Step 5️: Persist the updated tenant_attr
    UPDATE tenant.tenant
    SET tenant_attr = v_tenant_attr
    WHERE tenant_id = v_tenant_id;

    RAISE NOTICE '[Information] Rollback completed successfully for tenant_id %', v_tenant_id;
END $$;
