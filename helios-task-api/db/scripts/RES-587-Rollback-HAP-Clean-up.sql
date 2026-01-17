-- ============================================================================
-- 🚀 Script    : Rollback Script for HAP-TENANT-CODE task clean-up
-- 📌 Purpose   : To rollback soft delete (set delete_nbr back to 0)
-- 🧑 Author    : Siva
-- 📅 Date      : 2025-09-26
-- 🧾 Jira      : RES-587
-- ⚠️ Inputs    : HAP-TENANT-CODE
-- 📤 Output    : Restores soft deleted task_reward records
-- 🔗 Script URL: NA
-- 📝 Notes     : This rollback is only for updates done with update_user = 'RES-587'
-- ============================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>';  -- Input tenant_code
    v_restored BIGINT;
BEGIN
    UPDATE task.task_reward tr
    SET delete_nbr = 0,
        update_ts = NOW(),
        update_user = 'SYSTEM'
    WHERE tr.tenant_code = v_tenant_code
      AND tr.update_user = 'RES-587'

    GET DIAGNOSTICS v_restored = ROW_COUNT;

    RAISE NOTICE 'Rollback complete. Restored % records for tenant %.', v_restored, v_tenant_code;
END $$;
