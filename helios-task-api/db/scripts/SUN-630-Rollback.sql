-- SUN-630: Update task_header and task_description in task.task_detail for Trivia
-- Replace the input parameters before executing

DO $$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>';  -- Replace with actual tenant code
    
    v_mapping JSONB := '[
		{
			"taskExternalCode" : "play_heal_triv",
			"enUSTaskHeader" : "Play healthy trivia",
			"esTaskHeader": "Participe en la trivia de salud",
			"enUSDescription": "Play healthy trivia and learn fun facts. Answer questions about food, fitness, wellness and more to earn a chance to win. It''s a fun way to stay healthy and build your smarts!",
			"esDescription": "Juega trivias saludables y aprende datos curiosos. Responde preguntas sobre nutrición, fitness, bienestar y más para tener la oportunidad de ganar. ¡Es una forma divertida de mantenerte sano y desarrollar tu inteligencia!"
		},
		{
			"taskExternalCode" : "play_week_heal_triv",
			"enUSTaskHeader" : "Play weekly trivia",
			"esTaskHeader": "Participe en la trivia de salud",
			"enUSDescription": "Play weekly trivia and learn fun facts. Answer questions about food, fitness, wellness and more to earn a chance to win. It''s a fun way to stay healthy and build your smarts!",
			"esDescription": "Juega a la trivia semanal y aprende datos curiosos. Responde preguntas sobre nutrición, fitness, bienestar y más para tener la oportunidad de ganar. ¡Es una forma divertida de mantenerte sano y desarrollar tu inteligencia!"
		},
		{
			"taskExternalCode" : "play_dail_heal_triv",
			"enUSTaskHeader" : "Play daily trivia"
			"esTaskHeader": "Participe en la trivia de salud",
			"enUSDescription": "Play daily trivia and learn fun facts. Answer questions about food, fitness, wellness and more to earn a chance to win. It''s a fun way to stay healthy and build your smarts!",
			"esDescription": "Juega a trivias a diario y aprende datos curiosos. Responde preguntas sobre nutrición, fitness, bienestar y más para tener la oportunidad de ganar. ¡Es una forma divertida de mantenerte sano y desarrollar tu inteligencia!"
		}
	]';

    item JSONB;
    v_task_external_code TEXT;
    v_en_header TEXT;
    v_es_header TEXT;
    v_en_description TEXT;
    v_es_description TEXT;

    v_task_id BIGINT;
    v_task_detail_id BIGINT;
    v_lang TEXT;
	v_update_user TEXT := 'SYSTEM';  
    rows_updated INT;
BEGIN
    -- Loop through each item in the JSON array
    FOR item IN SELECT * FROM jsonb_array_elements(v_mapping)
    LOOP
        -- Extract fields from the JSON object
        v_task_external_code := item ->> 'taskExternalCode';
        v_en_header := item ->> 'enUSTaskHeader';
        v_es_header := item ->> 'esTaskHeader';
        v_en_description := item ->> 'enUSDescription';
        v_es_description := item ->> 'esDescription';

        RAISE NOTICE 'Processing taskExternalCode: % for tenant: %', v_task_external_code, v_tenant_code;

        -- Get matching task_ids for the given tenant and external code
        FOR v_task_id IN
            SELECT task_id
            FROM task.task_reward
            WHERE tenant_code = v_tenant_code
              AND task_external_code = v_task_external_code
              AND delete_nbr = 0
        LOOP
            RAISE NOTICE 'Found task_id: % for taskExternalCode: %', v_task_id, v_task_external_code;

            rows_updated := 0;

            -- Loop over matching task_detail records
            FOR v_task_detail_id, v_lang IN
                SELECT task_detail_id, language_code
                FROM task.task_detail
                WHERE tenant_code = v_tenant_code
                  AND task_id = v_task_id
                  AND delete_nbr = 0
            LOOP
                IF v_lang = 'en-US' THEN
                    UPDATE task.task_detail
                    SET task_header = v_en_header,
                        task_description = v_en_description,
                        update_ts = NOW(),
                        update_user = v_update_user
                    WHERE task_detail_id = v_task_detail_id;

                    RAISE NOTICE 'Updated EN record: tenant_code=%, task_id=%, task_detail_id=% → header="%", description="%"',
                        v_tenant_code, v_task_id, v_task_detail_id, v_en_header, v_en_description;

                    rows_updated := rows_updated + 1;

                ELSIF v_lang = 'es' THEN
                    UPDATE task.task_detail
                    SET task_header = v_es_header,
                        task_description = v_es_description,
                        update_ts = NOW(),
                        update_user = v_update_user
                    WHERE task_detail_id = v_task_detail_id;

                    RAISE NOTICE 'Updated ES record: tenant_code=%, task_id=%, task_detail_id=% → header="%", description="%"',
                        v_tenant_code, v_task_id, v_task_detail_id, v_es_header, v_es_description;

                    rows_updated := rows_updated + 1;

                ELSE
                    RAISE NOTICE 'Skipping unsupported language: % for task_detail_id: %', v_lang, v_task_detail_id;
                END IF;
            END LOOP;

            IF rows_updated = 0 THEN
                RAISE NOTICE 'No task_detail records found for task_id: % and tenant_code: %', v_task_id, v_tenant_code;
            END IF;
        END LOOP;
    END LOOP;
END $$;
