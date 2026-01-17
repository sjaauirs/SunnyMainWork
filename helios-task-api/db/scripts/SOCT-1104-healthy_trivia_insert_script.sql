DO
$$
DECLARE
	v_tenant_code VARCHAR := '<KP-TENANT-CODE>';  -- Replace with actual KP Tenant Code
	
    v_input_json JSONB :='{
  "trivia": {
    "triviaTaskExternalCode": "play_heal_triv",
    "ctaTaskExternalCode": "play_now",
    "config": {
      "ux": {
        "questionIcon": {
          "url": "",
          "bgColor": "#111111",
          "fgColor": "#FFFFFF"
        },
        "backgroundUrl": "",
        "wrongAnswerIcon": {
          "url": "",
          "bgColor": "#FF0000",
          "fgColor": "#FFFFFF"
        },
        "correctAnswerIcon": {
          "url": "",
          "bgColor": "#111111",
          "fgColor": "#FFFFFF"
        }
      }
    }
  },
  "triviaQuestions": [
    {
      "questionExternalCode": "where_can_you_find_over_4_000_heal_topi_on_medi_condi_symp_and_p_first",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Nuestra enciclopedia de salud le brinda la información que necesita para aprender los conceptos básicos, cuidarse o recibir atención."
      },
      "answerText": [
        "One Pass Select Affinity",
        "Una Bola 8 Mágica",
        "Nuestra enciclopedia de salud"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "¿Dónde puede encontrar más de 4,000 temas de salud sobre afecciones médicas, síntomas y procedimientos?",
      "correctAnswer": [
        2
      ],
      "validStartTs": "2025-06-21T00:00:00Z",
      "validEndTs": "2026-01-01T23:59:59Z"
    },
    {
      "questionExternalCode": "which_device_helps_track_a_baby_s_heartbeat_during_pregnancy_first_kp_trivia",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Un monitor Doppler fetal utiliza ondas sonoras para comprobar los latidos del corazón del bebé. Ayuda a los médicos a asegurarse de que el bebé esté sano y crezca bien."
      },
      "answerText": [
        "Un monitor Doppler",
        "Un cardiotocógrafo",
        "Un walkie-talkie"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "¿Qué dispositivo ayuda a rastrear los latidos del corazón de un bebé durante el embarazo?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-06-21T00:00:00Z",
      "validEndTs": "2026-01-01T23:59:59Z"
    },
    {
      "questionExternalCode": "which_food_has_the_highest_fiber_per_serving_first_kp_trivia",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Los frijoles tienen un alto contenido de fibra y aportan proteínas al cuerpo. También tienen vitaminas y minerales importantes como hierro y zinc. Son bajos en grasas y no tienen colesterol."
      },
      "answerText": [
        "Los frijoles",
        "El pan blanco",
        "La caja de cereales"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "¿Qué alimento tiene más fibra por porción?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-06-21T00:00:00Z",
      "validEndTs": "2026-01-01T23:59:59Z"
    },
    {
      "questionExternalCode": "how_can_i_get_proof_of_immunization_before_traveling_first_kp_trivia",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Recuerde descargar o imprimir su certificado de vacunación antes de viajar. No se puede acceder por nuestra aplicación ni en kp.org a nivel internacional."
      },
      "answerText": [
        "Se pide por correo electrónico al médico.",
        "En kp.org o nuestra app",
        "Hay que rezar que esté en el cajón de papeles."
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "¿Cómo obtengo un comprobante de vacunación antes de un viaje?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-06-21T00:00:00Z",
      "validEndTs": "2026-01-01T23:59:59Z"
    },
    {
      "questionExternalCode": "can_you_get_your_covid_19_vacc_and_flu_shot_without_an_appo_first_kp_trivia",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Se pueden hacer visitas sin turno en la mayoría de las sucursales de Kaiser Permanente para miembros de 5 años o más."
      },
      "answerText": [
        "Sí",
        "No"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "¿Puedo recibir la vacuna contra la COVID-19 y contra la gripe sin turno?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-06-21T00:00:00Z",
      "validEndTs": "2026-01-01T23:59:59Z"
    },
    {
      "questionExternalCode": "what_is_an_example_of_self_care_first_kp_trivia",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "El autocuidado significa hacer cosas que disfruta para sentirse feliz y tranquilo."
      },
      "answerText": [
        "Comprar un yate por impulso",
        "Tomar un baño",
        "Pasar horas revisando las redes sociales"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "¿Cuál es un ejemplo de autocuidado?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-06-21T00:00:00Z",
      "validEndTs": "2026-01-01T23:59:59Z"
    },
    {
      "questionExternalCode": "what_is_a_generic_drug_first_kp_trivia",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Los medicamentos genéricos funcionan igual que los de marca, pero cuestan menos."
      },
      "answerText": [
        "Una medicamento sin personalidad",
        "Una alternativa menos costosa a un medicamento de marca",
        "Una medicamento con múltiples usos"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "¿Qué es un medicamento genérico?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-06-21T00:00:00Z",
      "validEndTs": "2026-01-01T23:59:59Z"
    }
  ]
}'::jsonb;

    
    v_task_external_code TEXT;
    v_cta_task_external_code TEXT;
    v_task_reward_id BIGINT;
    v_trivia_id BIGINT;
    v_trivia_code UUID;
    v_exists BOOLEAN;
    v_config_json JSONB;

    question JSONB;
    v_question_external_code TEXT;
    v_existing_code TEXT;
    v_existing_json JSONB;
    v_question_code TEXT;
    v_trivia_json JSONB;

    v_es_json JSONB;
    v_en_json JSONB;

    v_trivia_question_id BIGINT;
    v_sequence_nbr INT := 0;

    v_valid_start_ts TIMESTAMP;
    v_valid_end_ts TIMESTAMP;

BEGIN
    -- Extract task and config
    v_task_external_code := v_input_json -> 'trivia' ->> 'triviaTaskExternalCode';
    v_cta_task_external_code := v_input_json -> 'trivia' ->> 'ctaTaskExternalCode';
    v_config_json := v_input_json -> 'trivia' -> 'config';

    -- Fetch task_reward_id
    SELECT task_reward_id INTO v_task_reward_id
    FROM task.task_reward
    WHERE task_external_code = v_task_external_code
      AND tenant_code = v_tenant_code
      AND delete_nbr = 0;

    IF v_task_reward_id IS NULL THEN
        RAISE EXCEPTION '❌ task_reward_id not found for %', v_task_external_code;
    ELSE
        RAISE NOTICE '✅ Found task_reward_id: %', v_task_reward_id;
    END IF;

    -- Fetch or insert trivia
    SELECT trivia_id INTO v_trivia_id
    FROM task.trivia
    WHERE task_reward_id = v_task_reward_id
      AND cta_task_external_code = v_cta_task_external_code
      AND delete_nbr = 0;

    IF v_trivia_id IS NULL THEN
        RAISE NOTICE '⚠️ Trivia not exists with Task Reward Id: %', v_task_reward_id;
    ELSE
        RAISE NOTICE '⚠️ Trivia already exists with ID: %', v_trivia_id;
		UPDATE task.trivia set delete_nbr = trivia_id
		WHERE delete_nbr = 0 and trivia_id = v_trivia_id;
		
        RAISE NOTICE '❌ Soft-deleted existing Trivia with ID: %', v_trivia_id;
    END IF;
	
	--Insert Trivia
	v_trivia_code := gen_random_uuid();
	INSERT INTO task.trivia (
		trivia_code, task_reward_id, cta_task_external_code, config_json,
		create_ts, update_ts, create_user, update_user, delete_nbr
	) VALUES (
		'trv-' || v_trivia_code, v_task_reward_id, v_cta_task_external_code, v_config_json::jsonb,
		now(), NULL, 'SYSTEM', NULL, 0
	);
	SELECT trivia_id INTO v_trivia_id FROM task.trivia
	WHERE trivia_code = 'trv-' || v_trivia_code and delete_nbr = 0;
	RAISE NOTICE '✅ Inserted trivia with ID: %', v_trivia_id;

    -- Process trivia questions
    FOR question IN SELECT * FROM jsonb_array_elements(v_input_json -> 'triviaQuestions')
    LOOP
        RAISE NOTICE '--- Processing question #% ---', v_sequence_nbr + 1;

        v_question_external_code := lower(question ->> 'questionExternalCode');

        -- Extract ES block
        v_es_json := jsonb_build_object(
            'learning', question -> 'learning',
            'answerText', question -> 'answerText',
            'answerType', question ->> 'answerType',
            'layoutType', question ->> 'layoutType',
            'questionText', question ->> 'questionText',
            'correctAnswer', question -> 'correctAnswer',
            'questionExternalCode', question ->> 'questionExternalCode',
			'validStartTs',question ->> 'validStartTs',
			'validEndTs',question ->> 'validEndTs'
        );

        v_trivia_json := jsonb_build_object('es', v_es_json);

        -- Check if trivia_question exists
        SELECT trivia_question_code, trivia_json
        INTO v_existing_code, v_existing_json
        FROM task.trivia_question
        WHERE question_external_code = v_question_external_code
          AND delete_nbr = 0
        LIMIT 1;
		
		-- Extract validity timestamps
		IF v_existing_json IS NOT NULL AND (v_existing_json -> 'en-US') IS NOT NULL THEN
			v_valid_start_ts := (v_existing_json -> 'en-US' ->> 'validStartTs')::timestamp;
			v_valid_end_ts := (v_existing_json -> 'en-US' ->> 'validEndTs')::timestamp;
		ELSE
			v_valid_start_ts := (question ->> 'validStartTs')::timestamp;
			v_valid_end_ts := (question ->> 'validEndTs')::timestamp;
		END IF;

        IF v_existing_code IS NULL THEN
            v_question_code := 'trq-' || gen_random_uuid();
            INSERT INTO task.trivia_question (
                trivia_question_code, question_external_code, trivia_json,
                create_ts, update_ts, create_user, update_user, delete_nbr
            ) VALUES (
                v_question_code, v_question_external_code, v_trivia_json,
                now(), NULL, 'SYSTEM', NULL, 0
            );
            RAISE NOTICE '✅ Inserted new trivia_question: %', v_question_code;
        ELSE
            -- Always update the es node to the new value
			UPDATE task.trivia_question
			SET trivia_json = v_existing_json || jsonb_set(v_existing_json, '{es}', v_es_json),
				update_ts = now(),
				update_user = 'SYSTEM'
			WHERE trivia_question_code = v_existing_code AND delete_nbr = 0;

			RAISE NOTICE '🔁 Updated trivia_question % with new "es" block.', v_existing_code;
        END IF;

        -- Fetch trivia_question_id
        SELECT trivia_question_id INTO v_trivia_question_id
        FROM task.trivia_question
        WHERE question_external_code = v_question_external_code
          AND delete_nbr = 0
        LIMIT 1;

        -- Check if already linked in trivia_question_group
        SELECT EXISTS (
            SELECT 1 FROM task.trivia_question_group
            WHERE trivia_id = v_trivia_id
              AND trivia_question_id = v_trivia_question_id
              AND delete_nbr = 0
        ) INTO v_exists;

        IF NOT v_exists THEN
            INSERT INTO task.trivia_question_group (
                trivia_id, trivia_question_id, sequence_nbr,
                valid_start_ts, valid_end_ts,
                create_ts, update_ts, create_user, update_user, delete_nbr
            ) VALUES (
                v_trivia_id, v_trivia_question_id, v_sequence_nbr,
                v_valid_start_ts, v_valid_end_ts,
                now(), NULL, 'SYSTEM', NULL, 0
            );
            RAISE NOTICE '✅ Linked question to trivia group at sequence %', v_sequence_nbr;
        ELSE
            RAISE NOTICE '⚠️ Trivia question already linked in group, skipping.';
        END IF;

        v_sequence_nbr := v_sequence_nbr + 1;
    END LOOP;

    RAISE NOTICE '🎯 ✅ All trivia questions and group links processed successfully.';
END
$$;