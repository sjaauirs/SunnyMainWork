-- Script to insert KP task event IDs to task_external_mapping table
DO $$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>';
BEGIN
    INSERT INTO task.task_external_mapping (
        tenant_code,
        task_third_party_code,
        task_external_code,
        create_ts,
        create_user,
        delete_nbr
    )
    SELECT *
    FROM (
        VALUES
            (v_tenant_code, '6001', 'get_your_flu_shot', CURRENT_TIMESTAMP, 'SYSTEM', 0),
            (v_tenant_code, '6002', 'get_your_flu_shot', CURRENT_TIMESTAMP, 'SYSTEM', 0),
            (v_tenant_code, '8100', 'star_your_well_coac', CURRENT_TIMESTAMP, 'SYSTEM', 0),
            (v_tenant_code, '8101', 'star_your_well_coac', CURRENT_TIMESTAMP, 'SYSTEM', 0),
            (v_tenant_code, '8102', 'star_your_well_coac', CURRENT_TIMESTAMP, 'SYSTEM', 0),
            (v_tenant_code, '8103', 'star_your_well_coac', CURRENT_TIMESTAMP, 'SYSTEM', 0),
            (v_tenant_code, '8104', 'star_your_well_coac', CURRENT_TIMESTAMP, 'SYSTEM', 0),
            (v_tenant_code, '8105', 'star_your_well_coac', CURRENT_TIMESTAMP, 'SYSTEM', 0),
            (v_tenant_code, '15145', 'comp_the_tota_heal_asse', CURRENT_TIMESTAMP, 'SYSTEM', 0)
    ) AS new_records (tenant_code, task_third_party_code, task_external_code, create_ts, create_user, delete_nbr)
    WHERE NOT EXISTS (
        SELECT 1
        FROM task.task_external_mapping tem
        WHERE tem.tenant_code = new_records.tenant_code
          AND tem.task_third_party_code = new_records.task_third_party_code
    );

END $$;
