-- ===========================================================================
-- Author      : Pernati Rakesh
-- Purpose     : Soft delete tenant task category records by setting delete_nbr to tenant_task_category_id for matching entries
-- Jira Task   : SOCT-1595
-- ===========================================================================


DO $$
DECLARE
    v_tenant_code TEXT := '<WATCO-TENANT-CODE>'; -- 🔹 Input Tenant Code
    v_count INT;
BEGIN
    RAISE NOTICE '🔍 Checking for records to rollback (Tenant: "%")...', v_tenant_code;

    -- Count matching active records
    SELECT COUNT(*) INTO v_count
    FROM task.tenant_task_category ttc
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0
      AND task_category_id IN (
            SELECT tc.task_category_id
            FROM task.task_category tc
            WHERE tc.task_category_code IN (
                'tcc-fdb03f308cc2466ab57820833e8ed2cd', -- Preventive Care
                'tcc-10194555bfeb4545b7697df509d2637a', -- Health and Wellness
                'tcc-915c29cf8839465bbed54a33e6f20d57', -- Financial Wellness
                'tcc-37ffaf47abaa4b6a84828f4dd0cba7a3', -- Company Culture
                'tcc-7186dc274cc14788b64dddcc7f4f0656', -- Clinical Care Gap
                'tcc-69377045b0fe4762896c55670d04f3d7', -- Benefits
                'tcc-6c2f5b8c44fe453a9db13fd10f7320b7'  -- Behavioral Health
            )
      );

    IF v_count = 0 THEN
        RAISE WARNING '⚠ No active matching records found for rollback. Nothing to do.';
        RETURN;
    ELSE
        RAISE NOTICE '📄 Found % active matching record(s) to soft delete.', v_count;
    END IF;

    -- Perform rollback in safe transaction block
    BEGIN
        UPDATE task.tenant_task_category ttc
        SET delete_nbr = ttc.tenant_task_category_id,
            update_ts = NOW() AT TIME ZONE 'UTC',
            update_user = 'SYSTEM'
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0
          AND task_category_id IN (
                SELECT tc.task_category_id
                FROM task.task_category tc
                WHERE tc.task_category_code IN (
                    'tcc-fdb03f308cc2466ab57820833e8ed2cd',
                    'tcc-10194555bfeb4545b7697df509d2637a',
                    'tcc-915c29cf8839465bbed54a33e6f20d57',
                    'tcc-37ffaf47abaa4b6a84828f4dd0cba7a3',
                    'tcc-7186dc274cc14788b64dddcc7f4f0656',
                    'tcc-69377045b0fe4762896c55670d04f3d7',
                    'tcc-6c2f5b8c44fe453a9db13fd10f7320b7'
                )
          );

        RAISE NOTICE '✅ Rollback completed successfully. Soft deleted % record(s).', v_count;

    EXCEPTION
        WHEN OTHERS THEN
            RAISE EXCEPTION '❌ Rollback failed: %', SQLERRM;
    END;
END $$;
