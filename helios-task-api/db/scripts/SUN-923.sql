-- ============================================================================
-- 🚀 Script    : Add permission tag to tenant reward config json to task
-- 📌 Purpose   : Add permission:exercise tag to tenant reward config json for strengthen your body task
-- 🧑 Author    : Neel
-- 📅 Date      : 2025-12-12
-- 🧾 Jira      : SUN-923
-- ⚠️ Inputs    : <KP-TENANT-CODE>
-- 📤 Output    : Updates the JSONB structure with the new key-value pair
-- 🔗 Script URL: <NA>
-- 📝 Notes     : This script assumes the `tenant_attr` column is of type JSONB.
--               If the key already exists, it will be overwritten.
-- ============================================================================

DO $$
DECLARE
  v_task_external_code TEXT := 'stre_your_body%';
  v_tenant_codes TEXT[] := ARRAY[
        'KP-TENANT-CODE'
    ];
    v_tenant_code TEXT;
    v_permission_tag JSONB := '{
        "tags": [
          "permission:exercise"
        ]
      }'::JSONB;
  BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP         
      -- Add new key to tenant_attr JSONB
      UPDATE task.task_reward
      SET task_reward_config_json = v_permission_tag
      WHERE task_external_code ILIKE v_task_external_code
      AND tenant_code = v_tenant_code;

      -- Confirmation message
      RAISE NOTICE '[Information] Task reward config updated successfully for tenant: %', v_tenant_code;
    END LOOP;
END $$;