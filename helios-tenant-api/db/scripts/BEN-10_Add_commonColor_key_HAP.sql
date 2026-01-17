-- =====================================================================
-- Script  : Upsert a color into ux.commonColors
-- DB      : PostgreSQL
-- Table   : tenant.tenant
-- Column  : tenant_attr (JSONB)
-- Tenant  : ten-b4e920d3f6f74496ab533d1a9a8ef9e4
-- Purpose : If key exists → update value; if missing → create it.
-- Behavior:
--   - Creates ux.commonColors if missing.
--   - Always ensures the color key is set to the given value.
-- =====================================================================

DO $$
DECLARE
  v_tenant_code  text := '<HAP-Tenant-code>'; -- change if needed
  v_color_key    text := 'contentBgColor4';                         -- your new key
  v_color_value  text := '#F7F4F0';                               -- your desired value
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
      tenant_attr::jsonb,
      '{ux,commonColors}',
      COALESCE(tenant_attr::jsonb #> '{ux,commonColors}', '{}'::jsonb)
      || jsonb_build_object(v_color_key, v_color_value),
      true
  )
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;
END $$;
