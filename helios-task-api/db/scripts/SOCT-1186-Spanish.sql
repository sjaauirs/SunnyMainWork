-- ============================================================================
-- Script: Insert Translated Task Headers into task.task_detail (Spanish - es)
-- Purpose:
--   Inserts or updates Spanish translations of task headers and related info
--   in the task.task_detail table by referencing task_id from English source
--   entries (same tenant).
--
-- Behavior:
--   - Finds the task_id for each English `task_header` from the source tenant.
--   - Updates the matching Spanish entry if it already exists.
--   - If not found, inserts a new entry using the same task_id.
--   - Associates translated headers, CTAs, and descriptions.
--   - Fetches the latest applicable terms_of_service_id for Spanish with
--     matching content (e.g., "Kaiser Permanente").
--   - Logs each step (insert, update, or failure) using RAISE NOTICE.
--
-- Parameters:
--   - v_source_tenant_code : Tenant code of the English source content
--   - v_target_tenant_code : Tenant code of the Spanish target content
--   - v_source_language    : 'en-US'
--   - v_target_language    : 'es'
--   - v_source_headers     : List of English task headers (source keys)
--   - v_target_headers     : Translated Spanish headers
--   - v_target_ctas        : Spanish CTA button text
--   - v_target_descriptions: Translated Spanish descriptions (escaped strings)
--
-- Notes:
--   - The `task_id` is reused across translations to ensure linkage.
--   - Will skip tasks not found in English source.
--   - The input arrays must be index-aligned across source and target data.
--
-- Jira Ticket: SOCT-1186 - KP Action Catalog for Spanish
-- Author     : Vinod Ullaganti
-- Created On : 16 July 2025
-- ============================================================================

DO $$
DECLARE
    v_source_tenant_code TEXT := '<KP TENANT_CODE>'; -- English -- KP Small CAP 2025: ten-353ae621abde4e22be409325a1dd0eab , KP Small CAP 2025-QA: ten-153bd6c47ebe4673a75c71faa22b9eb6
    v_target_tenant_code TEXT := '<KP TENANT_CODE>'; -- Spanish -- KP Small CAP 2025: ten-353ae621abde4e22be409325a1dd0eab , KP Small CAP 2025-QA: ten-153bd6c47ebe4673a75c71faa22b9eb6
    v_source_language TEXT := 'en-US';
    v_target_language TEXT := 'es';
    v_create_user TEXT := 'SYSTEM';
    v_terms_of_service_id BIGINT;
    v_i INT;
    v_terms_match_text TEXT := 'Kaiser Permanente';

    -- Arrays (index-aligned)
    v_source_headers TEXT[] := ARRAY[
		'Play weekly trivia', -- 01
        'Play daily trivia', -- 02
        'Play healthy trivia', -- 03
		'Complete the Total Health Assessment', -- 04
		'Start your wellness coaching', -- 05
		'Get your flu vaccine', -- 06
		'Select your health adventure', -- 07
		'Step it up', -- 08 
		'Strengthen your body', -- 09
		'Track Your Sleep', -- 10
		'Meditate to boost your wellness', -- 11
        'Be mindful of what you eat', -- 12
        'Rethink your drink', -- 13
		'Try our healthy recipes', -- 14
        'Share your feedback' -- 15
        ];

    v_target_headers TEXT[] := ARRAY[
		'Juegue la trivia de salud', -- 01
        'Juegue la trivia de salud', -- 02
        'Juegue la trivia de salud', -- 03
		'Complete la evaluación de salud total', -- 04
		'Comience su coaching de bienestar', -- 05
		'Vacúnese contra la gripe', -- 06
		'Elija su aventura de salud', -- 07
		'Hora de contar los pasos', -- 08
		'Fortalezca el cuerpo', -- 09
		'Descanse bien', -- 10
		'Medite para mejorar el bienestar', -- 11
        'Preste atención a lo que come', -- 12
        'Reflexione sobre lo que toma', -- 13
		'Pruebe nuestras recetas saludables', -- 14
        'Comparta sus comentarios' -- 15
    ];

    v_target_ctas TEXT[] := ARRAY[
        'Jugar ahora', -- 01
        'Jugar ahora', -- 02
        'Jugar ahora', -- 03
		'Empezar ahora', -- 04
        'Programar ahora', -- 05
        'Programar ahora', -- 06
        'Elegir ahora', -- 07
        'Más información', -- 08
        'Más información', -- 09
        'Recibir consejos', -- 10
        'Más información', -- 11
        'Recibir consejos', -- 12
        'Más información', -- 13
        'Empezar a planificar', -- 14
        'Completar ahora' -- 15
    ];

	-- Task descriptions (escaped JSON string format) Input Source arrays (index-based mapping) 
	v_target_descriptions TEXT[] := ARRAY[
		'Juegue la trivia de salud y aprenda datos divertidos. Responda preguntas sobre alimentación, fitness, bienestar y más para tener la oportunidad de ganar. ¡Es una forma divertida de mantenerse saludable y desarrollar la inteligencia!', -- 01
		'Juegue la trivia de salud y aprenda datos divertidos. Responda preguntas sobre alimentación, fitness, bienestar y más para tener la oportunidad de ganar. ¡Es una forma divertida de mantenerse saludable y desarrollar la inteligencia!', -- 02
		'Juegue la trivia de salud y aprenda datos divertidos. Responda preguntas sobre alimentación, fitness, bienestar y más para tener la oportunidad de ganar. ¡Es una forma divertida de mantenerse saludable y desarrollar la inteligencia!', -- 03
		'El conocimiento es poder. Haga la evaluación de salud total para controlar su salud y recibir consejos para alcanzar sus objetivos de bienestar. Las recompensas se agregarán automáticamente a la tarjeta de recompensas menos de 10 días después de completar la evaluación.', -- 04
		'Reciba orientación y apoyo personalizado de un entrenador de bienestar dedicado para establecer objetivos, cumplirlos y ver resultados. Puede hacer todo esto desde la comodidad del hogar. Hable con su asesor por teléfono sobre cómo comer de forma más saludable, mejorar el estado físico, reducir el estrés y más. No se necesita una derivación. Programe su primera cita hoy. Las recompensas se agregarán automáticamente a la tarjeta de recompensas menos de 10 días después de la sesión de coaching.', -- 05
		'Protéjase y proteja a sus seres queridos durante esta temporada de gripe. Evite enfermarse, tener que ir al hospital y recibir facturas médicas inesperadas. Visite al médico o farmacia local para recibir la vacuna sin costo hoy. Las recompensas se agregarán automáticamente a la tarjeta de recompensas menos de 10 días después de recibir la vacuna.', -- 06
		'Elija la aventura de salud que más le inspire para mejorar la salud. Complete actividades y gane recompensas por cada paso que tome para estar más saludable. ¿Quiere hacer un cambio? No se preocupe, la aventura se puede cambiar más tarde.', -- 07
		'Camine más para mejorar la salud, tener menos estrés y recibir más recompensas. Cuente los pasos con su dispositivo preferido u otra herramienta y gane recompensas cuando alcance los 200,000 pasos cada mes.', -- 08
		'Fortalezca los músculos cada semana. Pruebe yoga, pilates, entrenamiento con pesas o cualquier actividad que le impulse a moverse. Sentirá más fuerza, menos dolor y mejor equilibrio. Use su aplicación preferida para registrar al menos 2 sesiones de entrenamiento de fuerza por semana y gane recompensas cuando complete 8 o más sesiones en un mes.', -- 09
		'[
			{
				"type": "paragraph",
				"data": {
					"text": "Los expertos recomiendan dormir entre 7 y 9 horas cada noche para tener una mente y un cuerpo sanos. Use su dispositivo preferido u otra herramienta para hacer el seguimiento del sueño. Después, registre al menos 7 horas en 4 o más noches por semana. Obtendrá recompensas después de alcanzar las 20 noches en un mes."
				}
			},
			{
				"type": "paragraph",
				"data": {
					"text": "Para un sueño reparador, pruebe estos sencillos consejos:"
				}
			},
			{
				"type": "list",
				"data": {
					"style": "unordered",
					"items": [
						"Establezca una hora fija para acostarse y otra para despertarse.",
						"Acuéstese y levántese dentro de los 30 minutos de su hora establecida.",
						"Mantenga su habitación oscura y fresca."
					]
				}
			}
		]', -- 10
		'Encuentre su zen con la meditación y la atención plena. Use recursos como la app Calm u otras herramientas para hacer el seguimiento de los minutos de meditación o atención plena. Registre al menos 35 minutos cada semana y gane recompensas cuando registre 150 minutos en un mes. ¡Son apenas 5 minutos al día!', -- 11
		'Haga el seguimiento de lo que come para tomar conciencia de sus hábitos. Registre lo que come, cuándo, el nivel de hambre y el estado de ánimo al comer durante al menos 4 días de la semana. Obtendrá recompensas por cada mes que registres 20 días o más.', -- 12
		'Reduzca el consumo de bebidas azucaradas (como refrescos, jugos y bebidas energéticas) y alcohol. Intente saciar la sed con agua u otras bebidas sin azúcar al menos 6 días a la semana. Haga el seguimiento de sus elecciones con su aplicación o herramienta preferida y gane recompensas cuando alcance los 24 días de tomar solo bebidas sin azúcar cada mes.', -- 13
		'Comer sano puede ser fácil y sabroso. Explore recetas de temporada o basadas en categorías para encontrar lo que le encanta. Suba fotos de 2 recetas preparadas cada mes para ganar recompensas.', -- 14
		'¡Queremos escuchar su opinión! Tómese unos minutos para completar una breve encuesta y contarnos cómo podemos mejorar nuestro programa de recompensas.' -- 15
	];

BEGIN
    SELECT terms_of_service_id INTO v_terms_of_service_id
    FROM task.terms_of_service
    WHERE language_code = v_target_language
      AND terms_of_service_text ILIKE '%' || v_terms_match_text || '%'
    ORDER BY terms_of_service_id
    LIMIT 1;

    FOR v_i IN 1..array_length(v_source_headers, 1) LOOP
        DECLARE
            v_task_id BIGINT;
        BEGIN
            -- Find source task_id
            SELECT task_id INTO v_task_id
            FROM task.task_detail
            WHERE tenant_code = v_source_tenant_code
              AND language_code = v_source_language
              AND LOWER(task_header) = LOWER(v_source_headers[v_i])
              AND delete_nbr = 0
            LIMIT 1;

            IF NOT FOUND THEN
                RAISE NOTICE '[FAILED] Source not found: "%" → "%"', v_source_headers[v_i], v_target_headers[v_i];
                CONTINUE;
            END IF;

            -- Try update
            UPDATE task.task_detail
            SET
                task_header = v_target_headers[v_i],
                task_description = v_target_descriptions[v_i],
                terms_of_service_id = v_terms_of_service_id,
                task_cta_button_text = v_target_ctas[v_i],
                create_ts = NOW(),
                create_user = v_create_user
            WHERE task_id = v_task_id
              AND tenant_code = v_target_tenant_code
              AND language_code = v_target_language
              AND delete_nbr = 0;

            IF FOUND THEN
                RAISE NOTICE '[UPDATED] % → %', v_source_headers[v_i], v_target_headers[v_i];
            ELSE
                -- Insert if not updated
                INSERT INTO task.task_detail (
                    tenant_code, task_id, task_header, task_description,
                    terms_of_service_id, language_code, task_cta_button_text,
                    create_user, create_ts, delete_nbr
                ) VALUES (
                    v_target_tenant_code, v_task_id, v_target_headers[v_i],
                    v_target_descriptions[v_i], v_terms_of_service_id,
                    v_target_language, v_target_ctas[v_i],
                    v_create_user, NOW(), 0
                );
                RAISE NOTICE '[INSERTED] % → %', v_source_headers[v_i], v_target_headers[v_i];
            END IF;
        END;
    END LOOP;
END $$;
