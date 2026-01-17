-- 🚀 Script      : RES-54_update_tenant_attr-Commoncolors.sql
-- 📌 Purpose     : Add/Update ux.commonColors.textColor and buttonDisableColor
-- 🧑 Author      : Deepthi Muttineni
-- 📅 Date        : 2025-09-26
-- 🧾 Jira        : RES-54
-- ⚠️ Inputs      : tenant_code (KP tenant only)
-- 📤 Output      : Updates or inserts keys into tenant_attr.ux.commonColors
-- =============================================================================
-- Purpose : Add/Update ux.commonColors.textColor and buttonDisableColor
-- Tenant  : KP only
-- Notes   : Updates or inserts keys into tenant_attr.ux.commonColors
-- Jira    : RES-54
-- =============================================================================
DO
$$
DECLARE
   v_tenant_code TEXT := '<KP-TENANT-CODE>'; -- replace with actual KP tenant_code
   v_commonColors JSONB := '{
     "textColor": "#0D1C3D",
     "buttonDisableColor": "#858D9C"
   }';
BEGIN
   UPDATE tenant.tenant
   SET tenant_attr = jsonb_set(
       tenant_attr::jsonb,
       '{ux,commonColors}',
       coalesce(tenant_attr::jsonb #> '{ux,commonColors}', '{}'::jsonb) || v_commonColors,
       true
   )
   WHERE tenant_code = v_tenant_code;
END;
$$;

