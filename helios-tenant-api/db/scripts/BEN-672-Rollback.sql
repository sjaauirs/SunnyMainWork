-- ============================================================================
-- 🚀 Script    : Rollback - Update cardIssueFlowType with dynamic cohort codes
-- 📌 Purpose   : Rollback - Replaces cardIssueFlowType array of strings with array of objects
--                Fills cohortCode arrays with IDs from cohort.cohort
-- 🧑 Author    : Preeti
-- 📅 Date      : 09/29/2025
-- 🧾 Jira      : BEN-672
-- ⚠️ Inputs    : v_tenant_code (Tenant code)
-- 📤 Output    : Updated tenant_option_json
-- ============================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>'; -- replace with actual tenant
BEGIN
    UPDATE tenant.tenant
    SET tenant_option_json = jsonb_set(
        tenant_option_json,
        '{benefitsOptions,cardIssueFlowType}',
        '["IMMEDIATE"]'::jsonb,
        false
    )
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0;

    RAISE NOTICE 'Rolled back cardIssueFlowType to ["IMMEDIATE"] for tenant %', v_tenant_code;
END $$;

---

DO $$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>'; -- replace with actual tenant
BEGIN
    UPDATE tenant.tenant
    SET tenant_option_json = jsonb_set(
        tenant_option_json,
        '{benefitsOptions,cardIssueFlowType}',
        '["TASK_COMPLETION_CHECK"]'::jsonb,
        false
    )
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0;

    RAISE NOTICE 'Rolled back cardIssueFlowType to ["IMMEDIATE"] for tenant %', v_tenant_code;
END $$;

---

DO $$
DECLARE
    v_tenant_code TEXT := '<SUNNY-TENANT-CODE>'; -- replace with actual tenant
BEGIN
    UPDATE tenant.tenant
    SET tenant_option_json = jsonb_set(
        tenant_option_json,
        '{benefitsOptions,cardIssueFlowType}',
        '["TASK_COMPLETION_CHECK"]'::jsonb,
        false
    )
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0;

    RAISE NOTICE 'Rolled back cardIssueFlowType to ["IMMEDIATE"] for tenant %', v_tenant_code;
END $$;

---

DO $$
DECLARE
    v_tenant_code TEXT := '<NAVITUS-TENANT-CODE>'; -- replace with actual tenant
BEGIN
    UPDATE tenant.tenant
    SET tenant_option_json = jsonb_set(
        tenant_option_json,
        '{benefitsOptions,cardIssueFlowType}',
        '["TASK_COMPLETION_CHECK"]'::jsonb,
        false
    )
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0;

    RAISE NOTICE 'Rolled back cardIssueFlowType to ["IMMEDIATE"] for tenant %', v_tenant_code;
END $$;