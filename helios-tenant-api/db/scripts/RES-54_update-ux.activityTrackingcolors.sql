-- 🚀 Script      : RES-54_update_tenant_attr-ActivityTrackingColors.sql
-- 📌 Purpose     : Add/Update ux.activityTrackingColors in tenant_attr
-- 🧑 Author      : Deepthi Muttineni
-- 📅 Date        : 2025-09-26
-- 🧾 Jira        : RES-54
-- ⚠️ Inputs      : tenant_code (KP tenant only)
-- 📤 Output      : Adds or updates the provided keys in tenant_attr.ux.activityTrackingColors
-- =============================================================================
-- Purpose : Add/Update ux.activityTrackingColors in tenant_attr
-- Tenant  : KP only
-- Notes   : Idempotent; updates only when needed
-- Jira    : RES-54
-- =============================================================================
DO $$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>'; -- replace with actual KP tenant_code
    v_new_colors JSONB := '{
      "arrowBgColor": "#FFFFFF",
      "arrowColor": "#0078B3",
      "selectedDateBgColor": "#ECF9FF",
      "selectedDate": "#005F99",
      "nonSelectedDay": "#4A546A",
      "dotColor": "#4A546A",
      "pipeline": "#0D1C3D",
      "buttonColor": "#0078B3",
      "updateTime": "#4A546A",
      "textColor": "#4A546A",
      "addActivityButton": "#0078B3",
      "bgColor": "#FFFFFF"
    }';
BEGIN
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
        tenant_attr::jsonb,
        '{ux,activityTrackingColors}',
        COALESCE(tenant_attr::jsonb #> '{ux,activityTrackingColors}', '{}'::jsonb) || v_new_colors,
        true
    )
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0;

    RAISE NOTICE 'ux.activityTrackingColors merged with new color keys for tenant %', v_tenant_code;
END $$;