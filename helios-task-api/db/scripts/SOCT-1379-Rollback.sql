-- Rollback: Script to insert KP task event IDs to task_external_mapping table
DO $$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>';
BEGIN
    UPDATE task.task_external_mapping
    SET delete_nbr = task_external_mapping_id,
        update_ts = CURRENT_TIMESTAMP,
        update_user = 'SYSTEM'
    WHERE (tenant_code, task_third_party_code) IN (
        VALUES
            (v_tenant_code, '6001'),
            (v_tenant_code, '6002'),
            (v_tenant_code, '8100'),
            (v_tenant_code, '8101'),
            (v_tenant_code, '8102'),
            (v_tenant_code, '8103'),
            (v_tenant_code, '8104'),
            (v_tenant_code, '8105'),
            (v_tenant_code, '15145')
    )
    AND create_user = 'SYSTEM'
    AND delete_nbr = 0;

END $$;