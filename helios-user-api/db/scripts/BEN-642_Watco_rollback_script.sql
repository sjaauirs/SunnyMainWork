-- ============================================================================ 
-- 🚀 Script    : Rollback consumer_flow_progress and onboarding_progress_history
-- 📌 Purpose   : Deletes rows created by the backfill script (BEN-642) to restore 
--               `huser.consumer_flow_progress` and `huser.consumer_flow_progress_history` 
--               to their previous state for a given tenant/cohort.
-- 👨‍💻 Author   : Rakesh Pernati
-- 📅 Date      : 2025-10-08
-- 🧾 Jira      : BEN-642
-- ⚠️ Inputs    : 
--      v_tenant_code  → Tenant identifier
-- 📤 Output    : 
--      - Deleted consumer_flow_progress and corresponding history rows inserted by backfill
--      - Logs for deleted or skipped records
-- 🔗 Script URL: <Optional Confluence / GitHub link>
-- 📝 Notes     : 
--      - Safe to run if backfill was previously executed
--      - Skips rows not created by the backfill
-- ============================================================================


DO $$
DECLARE
    v_tenant_code TEXT := '<WATCO-TENANT-CODE>';   -- Change tenant code
    v_cohort_code TEXT := NULL;        
    v_flow_name   TEXT := 'onboarding_flow';       
    v_flow_pk BIGINT;
    v_deleted_history INT := 0;
    v_deleted_progress INT := 0;
BEGIN
    -- Find the flow
    SELECT pk
    INTO v_flow_pk
    FROM tenant.flow
    WHERE tenant_code = v_tenant_code
      AND flow_name = v_flow_name
      AND delete_nbr = 0
      AND (
          (cohort_code = v_cohort_code)
          OR (cohort_code IS NULL AND v_cohort_code IS NULL)
      )
    LIMIT 1;

    IF v_flow_pk IS NULL THEN
        RAISE WARNING '⚠️ No flow found for tenant %, cohort %, flow %',
            v_tenant_code, COALESCE(v_cohort_code,'<NULL>'), v_flow_name;
        RETURN;
    END IF;

    -- Step 1: Delete history records referencing SYSTEM-created flow_progress
    DELETE FROM huser.consumer_flow_progress_history h
    WHERE h.consumer_flow_progress_fk IN (
        SELECT pk
        FROM huser.consumer_flow_progress
        WHERE tenant_code = v_tenant_code
          AND flow_fk = v_flow_pk
          AND delete_nbr = 0
          AND create_user = 'SYSTEM_BACKFILL'
          AND (
              (cohort_code = v_cohort_code)
              OR (cohort_code IS NULL AND v_cohort_code IS NULL)
          )
    );

    GET DIAGNOSTICS v_deleted_history = ROW_COUNT;
    RAISE NOTICE '✔️ Deleted % rows from consumer_onboarding_progress_history', v_deleted_history;

    -- Step 2: Delete consumer_flow_progress rows
    DELETE FROM huser.consumer_flow_progress
    WHERE tenant_code = v_tenant_code
      AND flow_fk = v_flow_pk
      AND delete_nbr = 0
      AND create_user = 'SYSTEM_BACKFILL'
      AND (
          (cohort_code = v_cohort_code)
          OR (cohort_code IS NULL AND v_cohort_code IS NULL)
      );

    GET DIAGNOSTICS v_deleted_progress = ROW_COUNT;
    RAISE NOTICE '✔️ Deleted % rows from consumer_flow_progress', v_deleted_progress;

    -- Summary
    IF v_deleted_progress = 0 AND v_deleted_history = 0 THEN
        RAISE WARNING '⚠️ No rollback rows found for tenant %, cohort %, flow %',
            v_tenant_code, COALESCE(v_cohort_code,'<NULL>'), v_flow_name;
    ELSE
        RAISE NOTICE 'Rollback completed: % history rows, % progress rows deleted',
            v_deleted_history, v_deleted_progress;
    END IF;
END $$;
