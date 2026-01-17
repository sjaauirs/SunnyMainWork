-- ============================================================================
-- 🚀 Script    : SUN-702
-- 📌 Purpose   : update ux.mycardColors and ux.commonColors to original values
-- 🧑 Author    : Neel Kunchakurti
-- 📅 Date      : 2025-09-23
-- 🧾 Jira      : 702
-- ⚠️ Inputs    : Hap-Tenant-Code, Kp-Tenant-Code, Navitus-Tenant-Code
-- 📤 Output    : <Expected output/result, if applicable>
-- 🔗 Script URL: <Link to external documentation or reference, if an
-- 📝 Notes     : <Any additional notes or assumptions>

-- ============================================================================
DO $$
DECLARE
  hap_tenant_code text := '<HAP-TENANT-CODE>';
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
                      tenant_attr,
                      '{ux,mycardColors}',
                      '{
                         "orderButtonColor": "#FFFFFF",
                         "orderButtonLabelColor": "#181D27",
                         "orderButtonBorderColor": "#181D27",
                         "waitButtonColor": "#E9EBEE",
                         "waitButtonLabelColor": "#677083",
                         "waitButtonBorderColor": "transparent",
                         "activateButtonColor": "#FFFFFF",
                         "activateButtonLabelColor": "#181D27",
                         "activateButtonBorderColor": "#181D27"
                       }'::jsonb,
                      true   -- create if missing, replace if exists
                    )
  WHERE tenant_code = hap_tenant_code
    AND delete_nbr = 0;
END $$;

DO $$
DECLARE
  hap_tenant_code text := '<HAP-TENANT-CODE>';
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
                      tenant_attr,
                      '{ux,commonColors,dollar}',
                      '"#003B71"'::jsonb,
                      true
                    )
  WHERE tenant_code = hap_tenant_code
    AND delete_nbr = 0;
END $$;

DO $$
DECLARE
  hap_tenant_code text := '<KP-TENANT-CODE>';
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
                      tenant_attr,
                      '{ux,mycardColors}',
                      '{
                         "orderButtonColor": "#0078B3",
                         "orderButtonLabelColor": "#FFFFFF",
                         "orderButtonBorderColor": "transparent",
                         "waitButtonColor": "#E9EBEE",
                         "waitButtonLabelColor": "#677083",
                         "waitButtonBorderColor": "transparent",
                         "activateButtonColor": "#57A635",
                         "activateButtonLabelColor": "#FFFFFF",
                         "activateButtonBorderColor": "transparent"
                       }'::jsonb,
                      true   -- create if missing, replace if exists
                    )
  WHERE tenant_code = hap_tenant_code
    AND delete_nbr = 0;
END $$;

DO $$
DECLARE
  hap_tenant_code text := '<KP-TENANT-CODE>';
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
                      tenant_attr,
                      '{ux,commonColors,dollar}',
                      '"#003B71"'::jsonb,
                      true
                    )
  WHERE tenant_code = hap_tenant_code
    AND delete_nbr = 0;
END $$;

DO $$
DECLARE
  hap_tenant_code text := '<NAVITUS-TENANT-CODE>';
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
                      tenant_attr,
                      '{ux,mycardColors}',
                      '{
                         "orderButtonColor": "#0078B3",
                         "orderButtonLabelColor": "#FFFFFF",
                         "orderButtonBorderColor": "transparent",
                         "waitButtonColor": "#E9EBEE",
                         "waitButtonLabelColor": "#677083",
                         "waitButtonBorderColor": "transparent",
                         "activateButtonColor": "#57A635",
                         "activateButtonLabelColor": "#FFFFFF",
                         "activateButtonBorderColor": "transparent"
                       }'::jsonb,
                      true   -- create if missing, replace if exists
                    )
  WHERE tenant_code = hap_tenant_code
    AND delete_nbr = 0;
END $$;

DO $$
DECLARE
  hap_tenant_code text := '<NAVITUS-TENANT-CODE>';
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
                      tenant_attr,
                      '{ux,commonColors,dollar}',
                      '"#003B71"'::jsonb,
                      true
                    )
  WHERE tenant_code = hap_tenant_code
    AND delete_nbr = 0;
END $$;