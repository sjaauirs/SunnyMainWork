DO $$
BEGIN
    UPDATE task.task_reward
    SET task_completion_criteria_json = NULL
    WHERE tenant_code = '<tenant_code_hap>'
      AND task_external_code IN (
          'main_a_heal_bloo_pres',
          'comp_your_a1c_test',
          'comp_your_diab_eye_exam',
          'comp_a_reco_colo_scre',
          'comp_your_brea_canc_scre',
          'get_your_flu_vacc',
          'conn_with_your_navi'
      )
      AND delete_nbr = 0;

    RAISE NOTICE 'Rollback complete: criteria set to NULL for selected tasks';
END $$;
