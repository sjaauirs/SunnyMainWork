-- Note: Replace <KP-TENANT-CODE> with actual KP Tenant Code before execution

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
    v_task_reward_id  BIGINT;
    v_trivia_id BIGINT;
    question JSONB;
    v_question_external_code TEXT;
    v_trivia_question_id BIGINT;
    v_trivia_json JSONB;

    v_trivia_question_ids BIGINT[] := ARRAY[]::BIGINT[];

BEGIN
    -- Extract task and CTA codes
    v_task_external_code := v_input_json -> 'trivia' ->> 'triviaTaskExternalCode';
    v_cta_task_external_code := v_input_json -> 'trivia' ->> 'ctaTaskExternalCode';

    -- Fetch task_reward_id
    SELECT task_reward_id INTO v_task_reward_id
    FROM task.task_reward
    WHERE task_external_code = v_task_external_code
      AND tenant_code = v_tenant_code
      AND delete_nbr = 0;

    IF v_task_reward_id IS NULL THEN
        RAISE NOTICE '❌ task_reward_id not found, skipping rollback.';
        RETURN;
    END IF;

    -- Fetch trivia_id
    SELECT trivia_id INTO v_trivia_id
    FROM task.trivia
    WHERE task_reward_id = v_task_reward_id
      AND cta_task_external_code = v_cta_task_external_code
      AND delete_nbr = 0;

    IF v_trivia_id IS NULL THEN
        RAISE NOTICE '❌ No trivia record found for rollback.';
        RETURN;
    ELSE
        RAISE NOTICE '🔁 Rolling back data for trivia_id: %', v_trivia_id;
    END IF;

    -- Step 1: Process each question
    FOR question IN SELECT * FROM jsonb_array_elements(v_input_json -> 'triviaQuestions')
    LOOP
        v_question_external_code := lower(question ->> 'questionExternalCode');

        SELECT trivia_question_id, trivia_json INTO v_trivia_question_id, v_trivia_json
        FROM task.trivia_question
        WHERE question_external_code = v_question_external_code
          AND delete_nbr = 0
        LIMIT 1;

        IF v_trivia_question_id IS NOT NULL THEN
            IF jsonb_typeof(v_trivia_json) = 'object'
               AND (SELECT COUNT(*) FROM jsonb_object_keys(v_trivia_json)) = 1
               AND EXISTS (
                   SELECT 1 FROM jsonb_object_keys(v_trivia_json) AS k(key)
						WHERE k.key = 'es'
               ) THEN

                -- Only "es" exists — safe to delete
                DELETE FROM task.trivia_question_group
                WHERE trivia_question_id = v_trivia_question_id
                  AND trivia_id = v_trivia_id;

                RAISE NOTICE '🗑 Deleted trivia_question_group for: %', v_question_external_code;

                v_trivia_question_ids := array_append(v_trivia_question_ids, v_trivia_question_id);
            ELSE
                -- Other keys exist — remove only "es"
                UPDATE task.trivia_question
                SET trivia_json = v_trivia_json - 'es'
                WHERE trivia_question_id = v_trivia_question_id;

                RAISE NOTICE '✏️ Removed "es" key from trivia_json for: %', v_question_external_code;
            END IF;
        END IF;
    END LOOP;

    -- Step 2: Delete trivia if all questions were removable
    IF cardinality(v_trivia_question_ids) = jsonb_array_length(v_input_json -> 'triviaQuestions') THEN
        DELETE FROM task.trivia
        WHERE trivia_id = v_trivia_id;

        RAISE NOTICE '🗑 Deleted trivia record with ID: %', v_trivia_id;
    ELSE
        RAISE NOTICE '⚠️ Trivia not deleted because not all questions were removable.';
    END IF;

    -- Step 3: Delete eligible trivia questions
    FOREACH v_trivia_question_id IN ARRAY v_trivia_question_ids
    LOOP
        DELETE FROM task.trivia_question
        WHERE trivia_question_id = v_trivia_question_id;

        RAISE NOTICE '🗑 Deleted trivia_question ID: %', v_trivia_question_id;
    END LOOP;

    RAISE NOTICE '🎯 ✅ Rollback completed successfully.';
END
$$;
