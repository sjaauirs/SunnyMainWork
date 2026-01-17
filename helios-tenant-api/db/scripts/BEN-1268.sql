-- ============================================================================
-- 📌 Purpose   : Create or update onboarding flow and steps for NAV tenants
-- 🧑 Author    : Saurabh Jaiswal
-- 📅 Date      : 2025-11-13
-- 🧾 Jira      : BEN-1268
-- ⚙️ Inputs    : 
--                 • v_tenant_codes : Array of tenant codes to process.
--                 • v_flowSteps    : Ordered list of components.
-- ⚠️ Notes     :
--                 • Safe and idempotent.
-- ============================================================================

DO
$$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        '<NAV-TENANT_CODE>'
        -- Add more tenants as needed
    ];

    v_flow_name   TEXT := 'onboarding_flow';
    v_version_nbr INT  := 1;
    v_flowSteps TEXT[] := ARRAY[
        'rewards_splash_screen',
        'activate_card_model',
        'dob_verification_screen',
        'card_last_4_verification_screen',
        'card_activate_success_model',
        'notification_screen'
    ];

    v_effective_start_ts TIMESTAMPTZ := NOW();
    v_effective_end_ts   TIMESTAMPTZ := NULL;

    v_tenant_code TEXT;
    v_flow_pk BIGINT;
    v_step_pk BIGINT;
    v_step_idx INT;
    v_rowcount INT;
    v_current_component BIGINT;
    v_next_component BIGINT;
    v_name TEXT;
BEGIN
    RAISE NOTICE '================= FLOW CREATION START =================';

    FOREACH v_tenant_code IN ARRAY v_tenant_codes LOOP
        RAISE NOTICE '🏢 Processing tenant: %', v_tenant_code;

        SELECT pk INTO v_flow_pk
        FROM tenant.flow
        WHERE tenant_code = v_tenant_code
          AND flow_name = v_flow_name
          AND version_nbr = v_version_nbr
          AND delete_nbr = 0;

        IF v_flow_pk IS NULL THEN
            INSERT INTO tenant.flow (
                tenant_code, cohort_code, flow_name, version_nbr,
                effective_start_ts, effective_end_ts,
                create_ts, update_ts, create_user, update_user, delete_nbr
            )
            VALUES (
                v_tenant_code, NULL, v_flow_name, v_version_nbr,
                v_effective_start_ts, v_effective_end_ts,
                NOW(), NOW(), 'SYSTEM', NULL, 0
            )
            RETURNING pk INTO v_flow_pk;

            RAISE NOTICE '🎉 Inserted flow pk=% for tenant=%', v_flow_pk, v_tenant_code;
        ELSE
            UPDATE tenant.flow
            SET effective_start_ts = v_effective_start_ts,
                effective_end_ts   = v_effective_end_ts,
                update_ts          = NOW(),
                update_user        = 'SYSTEM'
            WHERE pk = v_flow_pk;
        END IF;

        FOR v_step_idx IN 1..array_length(v_flowSteps, 1) LOOP
            v_name := v_flowSteps[v_step_idx];

            SELECT pk INTO v_current_component
            FROM tenant.component_catalogue
            WHERE component_name = v_name AND delete_nbr = 0;

            IF v_step_idx < array_length(v_flowSteps, 1) THEN
                SELECT pk INTO v_next_component
                FROM tenant.component_catalogue
                WHERE component_name = v_flowSteps[v_step_idx + 1] AND delete_nbr = 0;
            ELSE
                v_next_component := NULL;
            END IF;

            UPDATE tenant.flow_step
            SET current_component_catalogue_fk    = v_current_component,
                on_success_component_catalogue_fk = v_next_component,
                on_failure_component_catalogue_fk = v_current_component,
                update_ts = NOW(), update_user = 'SYSTEM', delete_nbr = 0
            WHERE flow_fk = v_flow_pk
              AND step_idx = v_step_idx
            RETURNING pk INTO v_step_pk;

            IF v_step_pk IS NULL THEN
                INSERT INTO tenant.flow_step (
                    flow_fk, step_idx, current_component_catalogue_fk,
                    on_success_component_catalogue_fk, on_failure_component_catalogue_fk,
                    create_ts, update_ts, create_user, update_user, delete_nbr
                )
                VALUES (
                    v_flow_pk, v_step_idx, v_current_component,
                    v_next_component, v_current_component,
                    NOW(), NULL, 'SYSTEM', NULL, 0
                )
                RETURNING pk INTO v_step_pk;
            END IF;
        END LOOP;

        UPDATE tenant.flow_step
        SET delete_nbr = 1, update_ts = NOW(), update_user = 'SYSTEM'
        WHERE flow_fk = v_flow_pk
          AND step_idx > array_length(v_flowSteps, 1)
          AND delete_nbr = 0;

        GET DIAGNOSTICS v_rowcount = ROW_COUNT;
        RAISE NOTICE '✅ Flow complete for tenant=% (% steps, % cleaned)', v_tenant_code, array_length(v_flowSteps,1), v_rowcount;
    END LOOP;

    RAISE NOTICE '🏁 FLOW CREATION COMPLETE';
END
$$;