DO
$$
DECLARE
    v_component_type   TEXT := 'ui_component';
    v_ui_component_type BIGINT;
BEGIN
    -- Find component_type
    SELECT pk
    INTO v_ui_component_type
    FROM tenant.component_type
    WHERE component_type = v_component_type
      AND delete_nbr = 0;

    IF v_ui_component_type IS NOT NULL THEN
        -- Delete related component_catalogue entries first (FK dependency)
        DELETE FROM tenant.component_catalogue
        WHERE component_type_fk = v_ui_component_type
          AND component_name IN (
            'email_verification_screen',
            'rewards_splash_screen',
            'notification_screen',
            'dob_verification_screen',
            'card_last_4_verification_screen',
            'pick_a_purse_screen',
            'costco_actions_screen',
            'agreement_screen',
            'permission_screen',
            'onboarding_survey'
        )
          AND delete_nbr = 0;

        -- Delete component_type (if no longer needed)
        DELETE FROM tenant.component_type
        WHERE pk = v_ui_component_type
          AND component_type = v_component_type
          AND delete_nbr = 0;

        RAISE NOTICE '♻️ Rolled back component_type "%" and its components.', v_component_type;
    ELSE
        RAISE NOTICE '⚠️ component_type "%" not found. Nothing to rollback.', v_component_type;
    END IF;
END;
$$;
