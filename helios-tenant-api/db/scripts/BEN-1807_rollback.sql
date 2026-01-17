-- ============================================================================
-- 🚀 Script    : rollback_onboarding_step_config_multi_tenant.sql
-- 📌 Purpose   : Rollback script to clear step_config for onboarding flow steps
-- 🧑 Author    : Saurabh Jaiswal
-- 📅 Date      : 2025-11-13
-- 🧾 Jira      : BEN-1807
-- ⚙️ Notes     :
--                 • Sets step_config = NULL for specific onboarding components.
--                 • Safe to rerun multiple times (idempotent).
--                 • Must be executed after onboarding flow exists.
-- ============================================================================

DO
$$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        '<NAV-TENANT-CODE>'
        -- Add more tenants as needed
    ];

    v_tenant_code TEXT;
    v_reset_components TEXT[] := ARRAY[
        'activate_card_model',
        'dob_verification_screen',
        'card_last_4_verification_screen'
    ];
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes LOOP
        RAISE NOTICE '🔁 Rolling back step_config for tenant: %', v_tenant_code;

        UPDATE tenant.flow_step fs
        SET step_config = NULL,
            update_ts = NOW(),
            update_user = 'SYSTEM'
        FROM tenant.component_catalogue cc,
             tenant.flow f
        WHERE fs.current_component_catalogue_fk = cc.pk
          AND cc.component_name = ANY(v_reset_components)
          AND f.pk = fs.flow_fk
          AND f.tenant_code = v_tenant_code
          AND f.flow_name = 'onboarding_flow'
          AND f.delete_nbr = 0
          AND fs.delete_nbr = 0
          AND cc.delete_nbr = 0;

        GET DIAGNOSTICS v_tenant_code = ROW_COUNT;
        RAISE NOTICE '✅ Cleared step_config for tenant: %', v_tenant_code;
    END LOOP;

    RAISE NOTICE '🏁 Rollback complete for all tenants.';
END
$$;
