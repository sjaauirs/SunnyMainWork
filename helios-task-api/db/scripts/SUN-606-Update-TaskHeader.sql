-- SUN-606: This script updates the task_header in task.task_detail TABLE
-- Replace the input parameters before executing the script

DO $$
DECLARE
    v_tenant_code TEXT := '<KP_TENANT_CODE>';  -- Replace with actual tenant code

    v_mapping JSONB := '[
		{
			"fromHeader": "Inicia tu experiencia de recompensas",
			"toHeader": "Comience su experiencia de recompensas"
		},
		{
			"fromHeader": "Juegue la trivia de salud",
			"toHeader": "Participe en la trivia de salud"
		},
		{
			"fromHeader": "Medite para mejorar el bienestar",
			"toHeader": "Medite para mejorar su bienestar"
		},
		{
			"fromHeader": "Duerme tus z",
			"toHeader": "Duerma mejor"
		}
	]';
	v_language_code TEXT := 'es';
    item JSONB;
    from_header TEXT;
    to_header TEXT;
    v_task_detail_id BIGINT;
    rows_updated INT;
BEGIN
    FOR item IN SELECT * FROM jsonb_array_elements(v_mapping)
    LOOP
        from_header := item ->> 'fromHeader';
        to_header := item ->> 'toHeader';

        rows_updated := 0;

        FOR v_task_detail_id IN
            SELECT task_detail_id
            FROM task.task_detail
            WHERE task_header = from_header
              AND tenant_code = v_tenant_code
              AND language_code = v_language_code
              AND delete_nbr = 0
        LOOP
            UPDATE task.task_detail
            SET task_header = to_header
            WHERE task_detail_id = v_task_detail_id;

            RAISE NOTICE 'Updated task_detail_id: %, from: "%" → to: "%"', v_task_detail_id, from_header, to_header;
            rows_updated := rows_updated + 1;
        END LOOP;

        IF rows_updated = 0 THEN
            RAISE NOTICE 'No data found to update for tenant: %, header: "%"', v_tenant_code, from_header;
        END IF;
    END LOOP;
END $$;
