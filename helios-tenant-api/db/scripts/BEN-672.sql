-- ============================================================================
-- 🚀 Script    : Update cardIssueFlowType with dynamic cohort codes
-- 📌 Purpose   : Replaces cardIssueFlowType array of strings with array of objects
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
    v_immediate_cohorts JSONB;
    v_task_check_cohorts JSONB;
BEGIN
    -- Collect cohort IDs for IMMEDIATE flow
    SELECT jsonb_agg(cohort_code)
    INTO v_immediate_cohorts
    FROM cohort.cohort
    WHERE cohort_name IN ('hap_flex_rewards', 'henry_ford_health_flex_rewards')
      AND delete_nbr = 0;

    IF v_immediate_cohorts IS NULL THEN
        v_immediate_cohorts := '[]'::jsonb;
    END IF;

    -- Collect cohort IDs for TASK_COMPLETION_CHECK flow
    SELECT jsonb_agg(cohort_code)
    INTO v_task_check_cohorts
    FROM cohort.cohort
    WHERE cohort_name IN ('healthy_living_rewards')
      AND delete_nbr = 0;

    IF v_task_check_cohorts IS NULL THEN
        v_task_check_cohorts := '[]'::jsonb;
    END IF;

    -- Update tenant_option_json
    UPDATE tenant.tenant
    SET tenant_option_json = jsonb_set(
        tenant_option_json,
        '{benefitsOptions,cardIssueFlowType}',
        jsonb_build_array(
            jsonb_build_object(
                'flowType', 'IMMEDIATE',
                'cohortCode', v_immediate_cohorts
            ),
            jsonb_build_object(
                'flowType', 'TASK_COMPLETION_CHECK',
                'cohortCode', v_task_check_cohorts
            )
        ),
        false
    )
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0;

    RAISE NOTICE 'Updated cardIssueFlowType with cohort codes for tenant %', v_tenant_code;
END $$;

------

DO $$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>'; -- replace with actual tenant
BEGIN
    UPDATE tenant.tenant
    SET tenant_option_json = jsonb_set(
        tenant_option_json,
        '{benefitsOptions,cardIssueFlowType}',
        '[
          {"flowType": "TASK_COMPLETION_CHECK", "cohortCode": []}
        ]'::jsonb,
        false
    )
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0;

    RAISE NOTICE 'Updated cardIssueFlowType for tenant %', v_tenant_code;
END $$;

------

DO $$
DECLARE
    v_tenant_code TEXT := '<SUNNY-TENANT-CODE>'; -- replace with actual tenant
BEGIN
    UPDATE tenant.tenant
    SET tenant_option_json = jsonb_set(
        tenant_option_json,
        '{benefitsOptions,cardIssueFlowType}',
        '[
          {"flowType": "IMMEDIATE", "cohortCode": []}
        ]'::jsonb,
        false
    )
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0;

    RAISE NOTICE 'Updated cardIssueFlowType for tenant %', v_tenant_code;
END $$;

------

DO $$
DECLARE
    v_tenant_code TEXT := '<NAVITUS-TENANT-CODE>'; -- replace with actual tenant
BEGIN
    UPDATE tenant.tenant
    SET tenant_option_json = jsonb_set(
        tenant_option_json,
        '{benefitsOptions,cardIssueFlowType}',
        '[
          {"flowType": "IMMEDIATE", "cohortCode": []}
        ]'::jsonb,
        false
    )
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0;

    RAISE NOTICE 'Updated cardIssueFlowType for tenant %', v_tenant_code;
END $$;