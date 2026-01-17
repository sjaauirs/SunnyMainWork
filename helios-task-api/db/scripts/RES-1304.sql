-- ============================================================================
-- 🚀 Script    : Update Task description for Get moving 2026
-- 📌 Purpose   : Update task_description in task.task_detail table for matching 
--                tenant_code, task_external_code, and delete_nbr = 0 records.
-- 🧑 Author    : Neel Kunchakurti
-- 📅 Date      : 2025-12-02
-- 🧾 Jira      : RES-1303
-- ⚠️ Inputs    : 
--                1. KP Tenant Codes array
-- 📤 Output    : Updates task_description column for matching records.
-- 🔗 Script URL: NA
-- 📝 Notes     : 
--                - Ensure you run this in a transaction.
--                - This script assumes valid tenant codes and task_external_codes exist.
--                - delete_nbr = 0 is used to filter active tasks.
-- ============================================================================

DO $$
DECLARE
    -- Input array of tenant codes
    v_tenant_codes TEXT[] := ARRAY[, '<KP-TENANT-CODE>'];
	v_tenant_code TEXT;
    v_task_external_code TEXT :='get_movi_2026';
    v_tenant TEXT;
    v_task JSON;
    v_task_id BIGINT;
    v_task_description_en TEXT :='[{"type":"paragraph","data":{"text":"Get your heart pumping and enjoy greater energy. Move your way with a walk, a bike ride, a swim, or even a dance party in your kitchen."}},{"type":"paragraph","data":{"text":"\nTrack 30 minutes of moderate activity at least 5 days a week for a total of 600 minutes a month to earn rewards. Be sure to sync your device for easier tracking."}}]';

    v_task_description_es TEXT :='[{"data": {"text": "Acelere el ritmo de su corazón y disfrute de más energía. Póngase en movimiento con una caminata, un paseo en bicicleta, una clase de natación o incluso una fiesta de baile en la cocina."}, "type": "paragraph"}, {"data": {"text": "\nRegistre 30 minutos de actividad moderada al menos 5 días por semana hasta alcanzar un total de 600 minutos al mes y reciba las recompensas. No olvide sincronizar su dispositivo para que el seguimiento sea más fácil."}, "type": "paragraph"}]';
    
BEGIN
     RAISE NOTICE '🔄 Starting updates for tenant codes: %', v_tenant_codes;

    -- Loop through tenant codes
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        RAISE NOTICE '➡️ Processing tenant: %', v_tenant_code;

            SELECT task_id FROM task.task_reward
            WHERE 
            tenant_code = v_tenant_code
            and delete_nbr = 0 
            and
            task_external_code IN (v_task_external_code)
            INTO v_task_id;

            -- Update statement
            UPDATE task.task_detail
            SET task_description = CASE 
                                    WHEN language_code = 'en-US' THEN v_task_description_en
                                    WHEN language_code = 'es' THEN v_task_description_es
                                    ELSE task_description
                                END,
                update_ts = NOW()
            WHERE 
                task_id = v_task_id
                and delete_nbr = 0 
                and tenant_code like v_tenant_code;

            IF FOUND THEN
                RAISE NOTICE '✅ Updated task % for tenant % with task_description = %', v_task_external_code, v_tenant_code, v_task_description_en;
            ELSE
                RAISE NOTICE '⚠️ No record found for tenant % and task_external_code %', v_tenant_code, v_task_external_code;
            END IF;
    END LOOP;

    RAISE NOTICE '🏁 All updates completed.';

END;
$$;
