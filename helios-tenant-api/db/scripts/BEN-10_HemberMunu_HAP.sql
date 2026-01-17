
-- ===================================================================================
-- Purpose       : Update hamburgerMenu inside tenant_option_json for a specific tenant
-- Description   : Ensures benefitsOptions exists and sets a fixed list of menu items (excluding "shop")
-- JIRA Ticket   : BEN-10
-- ===================================================================================

DO $$
BEGIN
  UPDATE tenant.tenant
  SET tenant_option_json =
      jsonb_set(
        -- ensure benefitsOptions exists (merge an empty object when missing)
        COALESCE(tenant_option_json, '{}'::jsonb)
          || jsonb_build_object('benefitsOptions',
               COALESCE(tenant_option_json->'benefitsOptions','{}'::jsonb)
             ),
        '{benefitsOptions,hamburgerMenu}',
        '["myCard","myRewards","personal","notifications","manageCard","help","agreements","privacyPolicy","signOut"]'::jsonb,
        true  -- create missing
      )
  WHERE tenant_code = '<HAP-TENANT-CODE>'
    AND delete_nbr = 0;
END $$;

