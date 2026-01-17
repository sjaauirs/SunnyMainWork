-- ============================================================================
-- 🚀 Script    : Add forYouColors in tenant_attr
-- 📌 Purpose   : Adds/updates the forYouColors theme configuration inside tenant_attr
-- 🧑 Author    : Preeti
-- 📅 Date      : 2025-09-30
-- 🧾 Jira      : BEN-252
-- ⚠️ Inputs    : 
--    - v_tenant_code (Tenant Code, e.g., <TENANT-CODE>)
-- 📤 Output    : forYouColors JSON object added/updated in tenant_attr
-- 📝 Notes     : 
--   - Will merge new forYouColors config without removing existing keys in tenant_attr
--   - Rollback removes the entire forYouColors object
-- ============================================================================

DO $$
DECLARE
  v_tenant_code TEXT := '<HAP-TENANT-CODE>'; -- Replace with tenant code
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
                      COALESCE(tenant_attr, '{}'::jsonb),
                      '{ux,forYouColors}',
                      '{
                        "forYouCardsBgColor": "#F7F4F0",
                        "forYouCardsIconBgColor": "#FFFFFF",
                        "forYouCardsHeadingBgColor": "#181D27",
                        "forYouCardsButtonBgColor": "#FFFFFF",
                        "forYouCardsButtonLabelColor": "#181D27",
                        "forYouCardsButtonBorderColor": "#181D27",
                        "forYouModalButtonBgColor": "#FFFFFF",
                        "forYouModalButtonLabelColor": "#181D27",
                        "forYouModalButtonBorderColor": "#181D27",
                        "modalTextColor": "#181D27"
                      }'::jsonb,
                      true
                    )
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;

  RAISE NOTICE 'forYouColors added/updated for tenant %', v_tenant_code;
END $$;

DO $$
DECLARE
  v_tenant_code TEXT := '<KP-TENANT-CODE>'; -- Replace with tenant code
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
                      COALESCE(tenant_attr, '{}'::jsonb),
                      '{ux,forYouColors}',
                      '{
                        "forYouCardsBgColor": "#FFFFFF",
                        "forYouCardsIconBgColor": "#ECF9FF",
                        "forYouCardsHeadingBgColor": "#0D1C3D",
                        "forYouCardsButtonBgColor": "#0078B3",
                        "forYouCardsButtonLabelColor": "#FFFFFF",
                        "forYouCardsButtonBorderColor": "#0078B3",
                        "forYouModalButtonBgColor": "#0078B3",
                        "forYouModalButtonLabelColor": "#FFFFFF",
                        "forYouModalButtonBorderColor": "#0078B3",
                        "modalTextColor": "#0D1C3D"
                      }'::jsonb,
                      true
                    )
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;

  RAISE NOTICE 'forYouColors added/updated for tenant %', v_tenant_code;
END $$;
