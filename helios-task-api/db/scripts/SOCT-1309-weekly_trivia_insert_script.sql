-- Note: Replace <KP-TENANT-CODE> with actual KP Tenant Code before execution

DO
$$
DECLARE
	v_tenant_code VARCHAR := '<KP-TENANT-CODE>';  -- Replace with actual KP Tenant Code
	
    v_input_json JSONB :='{
  "trivia": {
    "triviaTaskExternalCode": "play_week_heal_triv",
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
      "questionExternalCode": "why_does_your_scalp_get_oily_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "El cuero cabelludo produce aceite natural (sebo) para mantener el cabello sano y evitar que la piel se reseque. Tal vez se est\u00e9 ba\u00f1ando m\u00e1s de lo necesario. Se recomienda hacerlo 3 veces por semana."
      },
      "answerText": [
        "Por las gl\u00e1ndulas seb\u00e1ceas",
        "Est\u00e1 en su fase rebelde",
        "Por la piel muerta"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfPor qu\u00e9 se pone graso el cuero cabelludo?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-06-21T00:00:00Z",
      "validEndTs": "2025-06-27T23:59:59Z"
    },
    {
      "questionExternalCode": "what_does_sunscreen_protect_your_skin_from_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Elija un protector solar que bloquee los rayos UVA y UVB. Esto ayuda a proteger contra las quemaduras solares, el da\u00f1o a la piel y el c\u00e1ncer de piel."
      },
      "answerText": [
        "Del viento",
        "De los rayos ultravioleta",
        "De los paparazzi"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfDe qu\u00e9 protege su piel el protector solar?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-06-21T00:00:00Z",
      "validEndTs": "2025-06-27T23:59:59Z"
    },
    {
      "questionExternalCode": "which_carries_oxygen_rich_blood_away_from_the_heart_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Las arterias transportan sangre rica en ox\u00edgeno desde el coraz\u00f3n hasta el resto del cuerpo. Las venas llevan sangre de regreso al coraz\u00f3n y a los pulmones para recibir m\u00e1s ox\u00edgeno."
      },
      "answerText": [
        "Las venas",
        "Las arterias",
        "Los conductores de las entregas a domicilio"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfCu\u00e1l transporta la sangre rica en ox\u00edgeno fuera del coraz\u00f3n?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-06-21T00:00:00Z",
      "validEndTs": "2025-06-27T23:59:59Z"
    },
    {
      "questionExternalCode": "how_many_cups_of_fruits_and_vegetables_combined_should_you_eat_daily_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Para reducir el riesgo de sufrir enfermedades card\u00edacas, diabetes, c\u00e1ncer y obesidad, coma muchas frutas y verduras. Tambi\u00e9n aportan vitaminas y fibra."
      },
      "answerText": [
        "\u00bfEl k\u00e9tchup cuenta?",
        "Al menos 1\u00a0taza",
        "Al menos 2.5\u00a0tazas"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfCu\u00e1ntas tazas de frutas y verduras combinadas debe comer diariamente?",
      "correctAnswer": [
        2
      ],
      "validStartTs": "2025-06-21T00:00:00Z",
      "validEndTs": "2025-06-27T23:59:59Z"
    },
    {
      "questionExternalCode": "what_makes_hair_grow_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "El cabello crece a partir de estructuras tubulares en la piel llamadas fol\u00edculos. Las c\u00e9lulas dentro del fol\u00edculo se dividen, se endurecen y se unen para formar lentamente una hebra de cabello."
      },
      "answerText": [
        "Gritar \u201c\u00a1Crece!\u201d",
        "C\u00e9lulas en los fol\u00edculos pilosos",
        "Gl\u00e1ndulas pilosas"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 hace que crezca el cabello?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-06-21T00:00:00Z",
      "validEndTs": "2025-06-27T23:59:59Z"
    },
    {
      "questionExternalCode": "what_important_substance_does_the_liver_produce_to_help_digest_fat_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "La bilis es producida por el h\u00edgado y ayuda a descomponer la grasa durante la digesti\u00f3n. Esto hace que sea m\u00e1s f\u00e1cil para el cuerpo absorber los nutrientes."
      },
      "answerText": [
        "Bilis",
        "Desengrasante",
        "\u00c1cido g\u00e1strico"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 sustancia importante produce el h\u00edgado para ayudar a digerir la grasa?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-06-21T00:00:00Z",
      "validEndTs": "2025-06-27T23:59:59Z"
    },
    {
      "questionExternalCode": "true_or_false_you_can_get_20_off_chiropractors_acupuncturists_and_massage_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Los miembros de Kaiser Permanente obtienen un 20\u00a0% de descuento en servicios musculoesquel\u00e9ticos cuando muestran su tarjeta de identificaci\u00f3n de miembro a un proveedor participante.\n\nLos servicios descritos anteriormente no est\u00e1n cubiertos por los beneficios de su plan de salud y no est\u00e1n sujetos a los t\u00e9rminos establecidos en su Evidencia de Cobertura u otros documentos del plan. Estos servicios pueden interrumpirse en cualquier momento sin previo aviso."
      },
      "answerText": [
        "FALSO",
        "VERDADERO"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "Verdadero o falso: Puede obtener un 20\u00a0% de descuento en quiropr\u00e1cticos, acupunturistas y masajistas con su plan.",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-06-21T00:00:00Z",
      "validEndTs": "2025-06-27T23:59:59Z"
    },
    {
      "questionExternalCode": "what_tool_can_help_you_determine_possible_health_issues_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "El verificador de s\u00edntomas de Kaiser Permanente ayuda a evaluar sus problemas de salud y a determinar cu\u00e1ndo recibir atenci\u00f3n."
      },
      "answerText": [
        "Biblioteca de cuestionarios en l\u00ednea de Kaiser Permanente",
        "Verificador de s\u00edntomas de Kaiser Permanente",
        "Secci\u00f3n de comentarios de un blog de salud"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 herramienta puede ayudar a determinar posibles problemas de salud?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-06-28T00:00:00Z",
      "validEndTs": "2025-07-04T23:59:59Z"
    },
    {
      "questionExternalCode": "what_happens_in_your_brain_when_you_feel_happy_or_loved_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "La oxitocina se libera cuando abraza a alguien, pasa tiempo con sus seres queridos o se siente conectado. \u00a1Vaya a buscar sus abrazos!"
      },
      "answerText": [
        "Estornuda",
        "Libera oxitocina",
        "Empieza a hacer listas de reproducci\u00f3n cursis"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 sucede en el cerebro cuando se siente feliz o amado?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-06-28T00:00:00Z",
      "validEndTs": "2025-07-04T23:59:59Z"
    },
    {
      "questionExternalCode": "can_caregivers_view_past_visit_information_for_family_members_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Los cuidadores pueden acceder a la mayor\u00eda de los servicios e informaci\u00f3n, pero el acceso var\u00eda seg\u00fan la edad de la persona cuidada y el estado en el que vive."
      },
      "answerText": [
        "S\u00ed, pero el acceso var\u00eda seg\u00fan la edad.",
        "S\u00ed, pueden ver todo.",
        "No, es ilegal compartir."
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfLos cuidadores pueden ver la informaci\u00f3n de visitas pasadas de los miembros de la familia?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-06-28T00:00:00Z",
      "validEndTs": "2025-07-04T23:59:59Z"
    },
    {
      "questionExternalCode": "how_many_times_per_week_should_you_engage_in_strength_training_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "A medida que envejecemos, los m\u00fasculos, los huesos y el equilibrio se debilitan. El entrenamiento de fuerza 2 a 3\u00a0veces por semana puede ayudar."
      },
      "answerText": [
        "De 2 a 3\u00a0veces por semana en d\u00edas no consecutivos",
        "A diario",
        "\u00bfEst\u00e1 contando bocadillos de queso?"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfCu\u00e1ntas veces por semana deber\u00eda realizar entrenamiento de fuerza?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-06-28T00:00:00Z",
      "validEndTs": "2025-07-04T23:59:59Z"
    },
    {
      "questionExternalCode": "how_do_you_refill_a_prescription_while_traveling_outside_a_kaiser_permane_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Si todav\u00eda le quedan reposiciones de la receta, puede obtenerlas en cualquier farmacia. Para buscar una farmacia cercana y transferir la receta, llame a la l\u00ednea de viajes al 951-268-3900."
      },
      "answerText": [
        "Mostrar la tarjeta de miembro a un farmac\u00e9utico.",
        "Ir a cualquier farmacia.",
        "Mostrar al farmac\u00e9utico una nota escrita a mano."
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfC\u00f3mo puede renovar una receta m\u00e9dica mientras viaja fuera del \u00e1rea de Kaiser Permanente?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-06-28T00:00:00Z",
      "validEndTs": "2025-07-04T23:59:59Z"
    },
    {
      "questionExternalCode": "a_temperature_above___is_generally_considered_a_fever_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "La fiebre se produce cuando el cuerpo combate una infecci\u00f3n. Por lo general, desaparece por si sola. Qu\u00e9dese en casa y alejado de otras personas hasta que pase."
      },
      "answerText": [
        "7\u00a0grados de separaci\u00f3n de Kevin Bacon",
        "104\u00a0grados",
        "100\u00a0grados"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "Una temperatura superior a ____ generalmente se considera fiebre.",
      "correctAnswer": [
        2
      ],
      "validStartTs": "2025-06-28T00:00:00Z",
      "validEndTs": "2025-07-04T23:59:59Z"
    },
    {
      "questionExternalCode": "what_is_cognitive_dissonance_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": ""
      },
      "answerText": [
        "Cuando alguien no puede distinguir la fantas\u00eda de la realidad",
        "Cuando su hor\u00f3scopo dice Libra aunque usted sea completamente Piscis",
        "Cuando las acciones no coinciden con las creencias y se siente inc\u00f3modo"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 es la disonancia cognitiva?",
      "correctAnswer": [
        2
      ],
      "validStartTs": "2025-06-28T00:00:00Z",
      "validEndTs": "2025-07-04T23:59:59Z"
    },
    {
      "questionExternalCode": "are_the_covid_19_vaccine_and_flu_shot_combined_into_one_shot_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Las vacunas se administran en dos inyecciones separadas porque protegen contra diferentes virus. Pero puede recibirlas con seguridad al mismo tiempo."
      },
      "answerText": [
        "No, son dos vacunas separadas.",
        "S\u00ed, a menudo se combinan.",
        "A veces, pero depende de cu\u00e1l consiga."
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfLa vacuna contra la COVID-19 y la vacuna contra la gripe se combinan en una sola inyecci\u00f3n?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-07-05T00:00:00Z",
      "validEndTs": "2025-07-11T23:59:59Z"
    },
    {
      "questionExternalCode": "what_should_you_do_if_someone_calls_asking_for_your_social_security_numbe_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Las agencias gubernamentales y las empresas reales nunca llaman para pedir datos personales. \u00a1Cuelguen el tel\u00e9fono si lo hacen!"
      },
      "answerText": [
        "Solo comp\u00e1rtalo si son amables",
        "Cuelgue y rep\u00f3rtelo",
        "D\u00edgales que es muy confidencial"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 debe hacer si alguien lo llama y le pide su n\u00famero de Seguro Social?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-07-05T00:00:00Z",
      "validEndTs": "2025-07-11T23:59:59Z"
    },
    {
      "questionExternalCode": "how_often_should_you_replace_your_toothbrush_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Reemplazar el cepillo de dientes cada 3 o 4\u00a0meses ayuda a mantener la boca limpia y libre de g\u00e9rmenes."
      },
      "answerText": [
        "Una vez al a\u00f1o",
        "Cada 3 a 4\u00a0meses",
        "Cuando parece un puercoesp\u00edn"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfCon qu\u00e9 frecuencia debe reemplazar su cepillo de dientes?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-07-05T00:00:00Z",
      "validEndTs": "2025-07-11T23:59:59Z"
    },
    {
      "questionExternalCode": "what_helps_regulate_your_sleep_cycle_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "La melatonina se libera cuando est\u00e1 oscuro y le indica al cuerpo que es hora de dormir. Aseg\u00farese de tener un entorno de descanso ideal."
      },
      "answerText": [
        "La melatonina",
        "El suave susurro de su almohada",
        "La serotonina"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 ayuda a regular el ciclo de sue\u00f1o?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-07-05T00:00:00Z",
      "validEndTs": "2025-07-11T23:59:59Z"
    },
    {
      "questionExternalCode": "what_is_the_best_way_to_prevent_athlete_s_foot_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Mantener los pies limpios, secos y con zapatos respirables ayuda a prevenir el pie de atleta."
      },
      "answerText": [
        "Instalar peque\u00f1os ventiladores de pie",
        "No usar medias",
        "Mantener los pies secos y limpios."
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfCu\u00e1l es la mejor manera de prevenir el pie de atleta?",
      "correctAnswer": [
        2
      ],
      "validStartTs": "2025-07-05T00:00:00Z",
      "validEndTs": "2025-07-11T23:59:59Z"
    },
    {
      "questionExternalCode": "is_care_at_a_student_health_center_covered_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Kaiser Permanente no cubre la atenci\u00f3n de rutina y no de emergencia en su centro de salud para estudiantes, por eso es importante verificar los detalles de su plan y explorar alternativas."
      },
      "answerText": [
        "Solo durante la semana del esp\u00edritu estudiantil.",
        "No, solo se cubre la atenci\u00f3n de emergencia.",
        "Solo atenci\u00f3n de rutina."
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfEst\u00e1 cubierta la atenci\u00f3n en un centro de salud para estudiantes?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-07-05T00:00:00Z",
      "validEndTs": "2025-07-11T23:59:59Z"
    },
    {
      "questionExternalCode": "how_can_you_tell_if_your_exercise_activity_is_vigorous_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Hay muchos ejercicios vigorosos que puede hacer. Entre ellos se incluyen trotar, correr, nadar y hacer ejercicios aer\u00f3bicos."
      },
      "answerText": [
        "Su reloj inteligente pide clemencia",
        "Debe hacer una pausa para respirar cuando habla.",
        "Est\u00e1 sudando"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfC\u00f3mo puede saber si su actividad f\u00edsica es vigorosa?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-07-05T00:00:00Z",
      "validEndTs": "2025-07-11T23:59:59Z"
    },
    {
      "questionExternalCode": "generic_prescription_drugs_can_save_you_money_and_are_just_as_effective_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Los medicamentos gen\u00e9ricos tienen los mismos ingredientes que los medicamentos de marca. Funcionan igual, pero cuestan menos."
      },
      "answerText": [
        "FALSO",
        "VERDADERO"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "Los medicamentos gen\u00e9ricos recetados pueden ahorrarle dinero y son igual de efectivos.",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-07-12T00:00:00Z",
      "validEndTs": "2025-07-18T23:59:59Z"
    },
    {
      "questionExternalCode": "what_does_the_liver_remove_from_the_blood_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "El h\u00edgado limpia la sangre eliminando toxinas y otras cosas que el cuerpo no necesita."
      },
      "answerText": [
        "Exceso de sal",
        "Malas ondas",
        "Toxinas"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 elimina el h\u00edgado de la sangre?",
      "correctAnswer": [
        2
      ],
      "validStartTs": "2025-07-12T00:00:00Z",
      "validEndTs": "2025-07-18T23:59:59Z"
    },
    {
      "questionExternalCode": "what_is_the_best_room_temperature_for_sleep_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Una habitaci\u00f3n fresca ayuda a dormir mejor, mientras que una calurosa lo hace m\u00e1s dif\u00edcil. El aire m\u00e1s fr\u00edo ayuda a sentir sue\u00f1o y a permanecer c\u00f3modo."
      },
      "answerText": [
        "No lo s\u00e9, estoy dormido",
        "Entre 80 y 90\u00a0grados",
        "Entre 60 y 69\u00a0grados"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfCu\u00e1l es la mejor temperatura ambiente para dormir?",
      "correctAnswer": [
        2
      ],
      "validStartTs": "2025-07-12T00:00:00Z",
      "validEndTs": "2025-07-18T23:59:59Z"
    },
    {
      "questionExternalCode": "what_tool_does_kaiser_permanente_offer_to_help_you_track_your_sleep_habit_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Un diario de sue\u00f1o permite identificar patrones que pueden afectar su calidad de sue\u00f1o y resultar en un mejor descanso."
      },
      "answerText": [
        "Un diario del sue\u00f1o",
        "Una lista de verificaci\u00f3n del sue\u00f1o",
        "Un oso de peluche robot que tome notas"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 herramienta ofrece Kaiser Permanente para ayudar con el seguimiento de los h\u00e1bitos de sue\u00f1o?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-07-12T00:00:00Z",
      "validEndTs": "2025-07-18T23:59:59Z"
    },
    {
      "questionExternalCode": "why_does_your_skin_get_wrinkly_in_water_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "La capa externa de piel, el estrato c\u00f3rneo, est\u00e1 formada por c\u00e9lulas muertas que pueden absorber agua. Est\u00e1 adherido a las capas subyacentes de la piel y crea una apariencia arrugada cuando se hincha."
      },
      "answerText": [
        "Los vasos sangu\u00edneos se cierran",
        "Se est\u00e1 convirtiendo en una pasa de uva",
        "La piel absorbe agua y se hincha"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfPor qu\u00e9 se arruga la piel con el agua?",
      "correctAnswer": [
        2
      ],
      "validStartTs": "2025-07-12T00:00:00Z",
      "validEndTs": "2025-07-18T23:59:59Z"
    },
    {
      "questionExternalCode": "how_much_moderate_physical_activity_should_you_get_each_week_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Estar activo mejora la salud f\u00edsica y mental. Es una de las mejores maneras de mantenerse saludable."
      },
      "answerText": [
        "\u00bfEste cuestionario cuenta como actividad?",
        "150\u00a0minutos",
        "300\u00a0minutos"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfCu\u00e1nta actividad f\u00edsica moderada deber\u00eda realizar cada semana?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-07-12T00:00:00Z",
      "validEndTs": "2025-07-18T23:59:59Z"
    },
    {
      "questionExternalCode": "why_does_your_heart_beat_faster_when_you_exercise_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Durante el ejercicio, el coraz\u00f3n late m\u00e1s r\u00e1pido para llevar m\u00e1s sangre y ox\u00edgeno al tejido muscular."
      },
      "answerText": [
        "Quiere seguir el ritmo de la lista de reproducci\u00f3n techno.",
        "Para enviar m\u00e1s ox\u00edgeno a los m\u00fasculos.",
        "Porque se est\u00e1 sobrecalentando"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfPor qu\u00e9 el coraz\u00f3n late m\u00e1s r\u00e1pido cuando hacemos ejercicio?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-07-12T00:00:00Z",
      "validEndTs": "2025-07-18T23:59:59Z"
    },
    {
      "questionExternalCode": "what_is_the_number_one_app_for_sleep_and_meditation_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Calm es la aplicaci\u00f3n n\u00famero uno para dormir y meditar y est\u00e1 dise\u00f1ada para reducir el estr\u00e9s, la ansiedad y m\u00e1s."
      },
      "answerText": [
        "Calm",
        "Smiling Mind",
        "Su aplicaci\u00f3n de streaming favorita"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfCu\u00e1l es la aplicaci\u00f3n n\u00famero uno para dormir y meditar?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-07-19T00:00:00Z",
      "validEndTs": "2025-07-25T23:59:59Z"
    },
    {
      "questionExternalCode": "which_part_of_your_body_produces_a_chemical_to_help_you_fall_asleep_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "La gl\u00e1ndula pineal del cerebro produce melatonina. Esto ayuda a regular el sue\u00f1o."
      },
      "answerText": [
        "El interruptor de apagado",
        "Las gl\u00e1ndulas suprarrenales",
        "La gl\u00e1ndula pineal"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 parte del cuerpo produce una sustancia qu\u00edmica que ayuda a conciliar el sue\u00f1o?",
      "correctAnswer": [
        2
      ],
      "validStartTs": "2025-07-19T00:00:00Z",
      "validEndTs": "2025-07-25T23:59:59Z"
    },
    {
      "questionExternalCode": "what_is_it_called_when_someone_pretends_to_be_a_company_or_person_you_tru_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "El phishing sucede cuando los estafadores lo enga\u00f1an para que proporcione informaci\u00f3n personal. A menudo lo hacen a trav\u00e9s de correos electr\u00f3nicos o mensajes de texto falsos."
      },
      "answerText": [
        "\u00a1Oh, cre\u00ed que ten\u00eda un admirador secreto!",
        "Phishing",
        "Hackeo"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfC\u00f3mo se llama cuando alguien se hace pasar por una empresa o persona de confianza para robar su informaci\u00f3n?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-07-19T00:00:00Z",
      "validEndTs": "2025-07-25T23:59:59Z"
    },
    {
      "questionExternalCode": "what_should_you_do_with_unused_medication_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "La eliminaci\u00f3n adecuada protege el medio ambiente y reduce el mal uso."
      },
      "answerText": [
        "Gu\u00e1rdelo para proyectos de manualidades",
        "D\u00e9selo a un amigo",
        "Deseche en un punto de recogida de medicamentos"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 hacer con los medicamentos no utilizados?",
      "correctAnswer": [
        2
      ],
      "validStartTs": "2025-07-19T00:00:00Z",
      "validEndTs": "2025-07-25T23:59:59Z"
    },
    {
      "questionExternalCode": "what_common_household_food_can_be_toxic_to_dogs_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "El chocolate contiene una sustancia llamada teobromina. Puede ser perjudicial para los perros y provocar v\u00f3mitos y convulsiones."
      },
      "answerText": [
        "Chocolate",
        "La tarea",
        "Pan"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 alimentos dom\u00e9sticos comunes pueden ser t\u00f3xicos para los perros?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-07-19T00:00:00Z",
      "validEndTs": "2025-07-25T23:59:59Z"
    },
    {
      "questionExternalCode": "how_are_infectious_diseases_such_as_cold_and_flu_most_spread_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Las enfermedades infecciosas como el resfriado y la gripe se propagan al toser, estornudar o hablar. \u00a1Es importante lavarse las manos y cubrirse la boca al toser o estornudar!"
      },
      "answerText": [
        "Por lavarse las manos",
        "Por gotitas respiratorias en el aire",
        "Por no actualizar el antivirus de la computadora"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfC\u00f3mo se propagan con mayor frecuencia las enfermedades infecciosas, como el resfriado y la gripe?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-07-19T00:00:00Z",
      "validEndTs": "2025-07-25T23:59:59Z"
    },
    {
      "questionExternalCode": "which_one_of_these_is_a_sign_of_burnout_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "El agotamiento nos hace sentir agotados e incapaces de recargar energ\u00edas. Detectarlo a tiempo puede ayudar a detenerlo."
      },
      "answerText": [
        "El insomnio",
        "El aumento de la productividad",
        "Pensar que un espresso doble cuenta como un desayuno equilibrado"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfCu\u00e1l de estos es un signo de agotamiento?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-07-19T00:00:00Z",
      "validEndTs": "2025-07-25T23:59:59Z"
    },
    {
      "questionExternalCode": "how_can_kaiser_permanente_help_you_if_you_need_care_while_traveling_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Kaiser Permanente ofrece una l\u00ednea de asistencia para viajes que ayuda a coordinar la atenci\u00f3n y el pago cuando est\u00e1 fuera de los EE.\u00a0UU. Tambi\u00e9n puede visitar kp.org/travel."
      },
      "answerText": [
        "Programar citas para usted.",
        "Hay ayuda disponible por nuestra l\u00ednea de viajes y en kp.org/travel",
        "Teletransportar a su m\u00e9dico."
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfC\u00f3mo puede Kaiser Permanente ayudar si necesita atenci\u00f3n mientras viaja?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-07-26T00:00:00Z",
      "validEndTs": "2025-08-01T23:59:59Z"
    },
    {
      "questionExternalCode": "what_tool_do_doctors_use_to_see_inside_a_joint_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Un artroscopio es una peque\u00f1a c\u00e1mara que ayuda a los m\u00e9dicos a ver el interior de las articulaciones. Les ayuda a encontrar lesiones y realizar cirug\u00edas con peque\u00f1os cortes."
      },
      "answerText": [
        "Un endoscopio",
        "Un artroscopio",
        "Una lupa y un poco de optimismo"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 herramienta utilizan los m\u00e9dicos para ver el interior de las articulaciones?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-07-26T00:00:00Z",
      "validEndTs": "2025-08-01T23:59:59Z"
    },
    {
      "questionExternalCode": "what_type_of_care_is_covered_when_traveling_outside_kaiser_permanente_are_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Tiene cobertura para servicios de urgencia o emergencia y atenci\u00f3n virtual, pero debe recibir servicios de rutina como controles antes del viaje."
      },
      "answerText": [
        "Controles de rutina y vacunas",
        "Paquetes de provisiones repletos de sopa",
        "Atenci\u00f3n de urgencia, de emergencia y virtual"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 tipo de atenci\u00f3n est\u00e1 cubierta cuando viajo fuera de las \u00e1reas de Kaiser Permanente?",
      "correctAnswer": [
        2
      ],
      "validStartTs": "2025-07-26T00:00:00Z",
      "validEndTs": "2025-08-01T23:59:59Z"
    },
    {
      "questionExternalCode": "consistent_good_quality_sleep_helps_improve_concentration_mood_immunity_a_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "El sue\u00f1o controla el hambre y las hormonas del metabolismo. Un sue\u00f1o saludable puede ayudar a perder peso y a mantenerlo estable."
      },
      "answerText": [
        "Mantener un peso saludable",
        "Una actitud positiva",
        "La masa muscular"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "Un sue\u00f1o bueno y de calidad de manera constante ayuda a mejorar la concentraci\u00f3n, el estado de \u00e1nimo, la inmunidad y ____________.",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-07-26T00:00:00Z",
      "validEndTs": "2025-08-01T23:59:59Z"
    },
    {
      "questionExternalCode": "after_you_meet_your_deductible_you_pay_a_percentage_of_health_care_costs__weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "El coaseguro es la forma en que usted comparte los costos de atenci\u00f3n m\u00e9dica con su plan. Cons\u00faltelo al elegir un plan para saber su costo."
      },
      "answerText": [
        "Copago",
        "Coseguro",
        "Propina"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "Una vez alcanzado su deducible, usted paga un porcentaje de los costos de atenci\u00f3n m\u00e9dica hasta una cierta cantidad. \u00bfC\u00f3mo se llama esto?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-07-26T00:00:00Z",
      "validEndTs": "2025-08-01T23:59:59Z"
    },
    {
      "questionExternalCode": "can_you_get_prescriptions_delivered_at_no_cost_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "La mayor\u00eda de las recetas se pueden entregar en un plazo de entre 3 y 7\u00a0d\u00edas h\u00e1biles, sin costo adicional con env\u00edo est\u00e1ndar."
      },
      "answerText": [
        "S\u00ed, para la mayor\u00eda de las recetas con env\u00edo est\u00e1ndar.",
        "No, el env\u00edo tiene un costo extra."
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfPuede recibir recetas a domicilio sin costo?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-07-26T00:00:00Z",
      "validEndTs": "2025-08-01T23:59:59Z"
    },
    {
      "questionExternalCode": "what_uses_light_to_treat_chronic_skin_conditions_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Un dispositivo de fototerapia ayuda con problemas de la piel como el eccema y la psoriasis. Puede calmar la piel y ayudar a sanar."
      },
      "answerText": [
        "Un term\u00f3metro infrarrojo",
        "La fototerapia",
        "La cama de bronceado"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 utiliza luz para tratar enfermedades cr\u00f3nicas de la piel?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-07-26T00:00:00Z",
      "validEndTs": "2025-08-01T23:59:59Z"
    },
    {
      "questionExternalCode": "how_many_ounces_of_water_should_you_consume_daily_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "El agua ayuda a la digesti\u00f3n, mantiene la regularidad intestinal y mantiene el cuerpo y la piel saludables."
      },
      "answerText": [
        "Al menos 84\u00a0onzas",
        "Al menos 64\u00a0onzas",
        "Pero \u00bfqu\u00e9 beber\u00e1n los pobres peces?"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfCu\u00e1ntas onzas de agua debe consumir diariamente?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-08-02T00:00:00Z",
      "validEndTs": "2025-08-08T23:59:59Z"
    },
    {
      "questionExternalCode": "what_causes_red_eye_in_flash_photos_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "El flash de una c\u00e1mara rebota en la retina, que cuenta con abundante suministro de sangre. Esto hace que los ojos se vean rojos en las fotograf\u00edas."
      },
      "answerText": [
        "La traici\u00f3n de la c\u00e1mara",
        "La luz que se refleja en la retina",
        "La sobreexposici\u00f3n"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 causa los ojos rojos en las fotograf\u00edas tomadas con flash?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-08-02T00:00:00Z",
      "validEndTs": "2025-08-08T23:59:59Z"
    },
    {
      "questionExternalCode": "true_or_false_the_kaiser_permanente_s_community_support_hub_features_reso_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Kaiser Permanente apoya todas sus necesidades de salud. Explore nuestro directorio de recursos para encontrar programas y servicios comunitarios en su \u00e1rea."
      },
      "answerText": [
        "FALSO",
        "VERDADERO"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "Verdadero o falso: El Community Support Hub\u2122 de Kaiser Permanente tiene recursos para apoyar su salud social, como ayuda con comida, vivienda y m\u00e1s.",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-08-02T00:00:00Z",
      "validEndTs": "2025-08-08T23:59:59Z"
    },
    {
      "questionExternalCode": "what_s_one_sign_a_website_might_be_fake_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Los sitios web fraudulentos suelen tener errores tipogr\u00e1ficos, URL extra\u00f1as e informaci\u00f3n de contacto falsa. Verifique siempre antes de ingresar su informaci\u00f3n."
      },
      "answerText": [
        "La URL parece extra\u00f1a",
        "Se carga lentamente",
        "Dice \u201cConf\u00ede en nosotros\u201d en letras grandes"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfCu\u00e1l es una se\u00f1al de que un sitio web podr\u00eda ser falso?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-08-02T00:00:00Z",
      "validEndTs": "2025-08-08T23:59:59Z"
    },
    {
      "questionExternalCode": "can_you_get_your_covid_19_vaccine_and_flu_shot_at_the_same_time_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Recibir la vacuna contra la gripe y la COVID-19 al mismo tiempo es seguro y conveniente."
      },
      "answerText": [
        "S\u00ed, es seguro recibir ambas a la vez.",
        "No, necesita al menos dos semanas de diferencia",
        "Solo durante la luna llena."
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfPuede recibir la vacuna contra la COVID-19 y contra la gripe al mismo tiempo?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-08-02T00:00:00Z",
      "validEndTs": "2025-08-08T23:59:59Z"
    },
    {
      "questionExternalCode": "what_lets_someone_make_health_care_decisions_for_you_if_you_can_t_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Un poder notarial para la atenci\u00f3n m\u00e9dica le permite a alguien tomar decisiones de tratamiento por usted. Esto es diferente de un testamento vital que establece sus deseos por escrito."
      },
      "answerText": [
        "Una nota adhesiva amarilla",
        "Un poder notarial para la atenci\u00f3n m\u00e9dica",
        "Un formulario de admisi\u00f3n"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 le permite a alguien tomar decisiones de atenci\u00f3n m\u00e9dica por usted si usted no puede hacerlo?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-08-02T00:00:00Z",
      "validEndTs": "2025-08-08T23:59:59Z"
    },
    {
      "questionExternalCode": "what_does_cbt_stand_for_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "La TCC es un tipo de terapia que ayuda a las personas a detectar y cambiar los pensamientos negativos. Puede ayudarles a sentirse mejor y a tomar decisiones m\u00e1s saludables."
      },
      "answerText": [
        "T\u00e9cnica para concentrarse en la calma",
        "Terapia cognitivo-conductual",
        "Tragar comida constantemente"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 significa TCC?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-08-02T00:00:00Z",
      "validEndTs": "2025-08-08T23:59:59Z"
    },
    {
      "questionExternalCode": "regular_exercise_can_reduce_your_risk_for_certain_diseases_such_as_heart__weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Haga 150\u00a0minutos de ejercicio aer\u00f3bico por semana y realice actividades de fuerza dos o m\u00e1s d\u00edas. Es bueno para la salud mental y f\u00edsica."
      },
      "answerText": [
        "VERDADERO",
        "FALSO"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "El ejercicio regular puede reducir el riesgo de padecer ciertas enfermedades, como enfermedades card\u00edacas, ciertos tipos de c\u00e1ncer y diabetes.",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-08-09T00:00:00Z",
      "validEndTs": "2025-08-15T23:59:59Z"
    },
    {
      "questionExternalCode": "how_many_americans_struggle_to_fall_or_stay_asleep_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Los problemas de sue\u00f1o afectan a muchas personas, pero hay recursos como las clases de sue\u00f1o de Kaiser Permanente que pueden ayudar a los miembros a mejorar sus h\u00e1bitos y calidad de sue\u00f1o."
      },
      "answerText": [
        "1 de cada 3",
        "1 de cada 5",
        "Depende de cu\u00e1ntos episodios queden de la serie"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfCu\u00e1ntos estadounidenses tienen dificultades para conciliar el sue\u00f1o o permanecer dormidos?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-08-09T00:00:00Z",
      "validEndTs": "2025-08-15T23:59:59Z"
    },
    {
      "questionExternalCode": "how_do_you_submit_a_claim_for_reimbursement_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Use kp.org para enviar reclamos de reembolso con todos los documentos necesarios."
      },
      "answerText": [
        "Inicia sesi\u00f3n en kp.org y hace clic en \u201cFacturaci\u00f3n\u201d.",
        "Env\u00eda los recibos por correo.",
        "Env\u00eda todo telep\u00e1ticamente."
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfC\u00f3mo presenta un reclamo de reembolso?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-08-09T00:00:00Z",
      "validEndTs": "2025-08-15T23:59:59Z"
    },
    {
      "questionExternalCode": "do_the_covid_19_vaccine_and_flu_shot_include_ingredients_that_aren_t_safe_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "La seguridad de las vacunas se eval\u00faa y se aprueba. Se usan ingredientes alimentarios comunes, como az\u00facares, sales y grasas, y millones de personas recibieron ambas dosis de forma segura."
      },
      "answerText": [
        "S\u00ed, pero es solo una peque\u00f1a cantidad.",
        "No, son seguros y efectivos.",
        "Los m\u00e9dicos todav\u00eda no est\u00e1n seguros."
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfLa vacuna contra la COVID-19 y la vacuna contra la gripe tienen ingredientes que no son seguros?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-08-09T00:00:00Z",
      "validEndTs": "2025-08-15T23:59:59Z"
    },
    {
      "questionExternalCode": "what_does_an_expiration_date_on_medication_mean_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Es posible que los medicamentos no funcionen tan bien despu\u00e9s de su vencimiento. Verifique siempre la fecha antes de usarlo y deseche de forma segura cualquier medicamento vencido."
      },
      "answerText": [
        "Que puede perder efectividad",
        "La fecha en que se convierte en vintage",
        "Que se vuelve t\u00f3xico"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 significa la fecha de caducidad en un medicamento?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-08-09T00:00:00Z",
      "validEndTs": "2025-08-15T23:59:59Z"
    },
    {
      "questionExternalCode": "when_is_the_best_time_to_get_your_flu_shot_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Es mejor vacunarse contra la gripe a finales de octubre, antes de que comience la temporada de gripe."
      },
      "answerText": [
        "Principios de oto\u00f1o",
        "Principios de primavera",
        "El d\u00eda antes de una gran fiesta"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfCu\u00e1l es el mejor momento para vacunarse contra la gripe?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-08-09T00:00:00Z",
      "validEndTs": "2025-08-15T23:59:59Z"
    },
    {
      "questionExternalCode": "what_does_one_pass_select_affinity_include_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "One Pass Select Affinity incluye beneficios de fitness y bienestar, como entrega de compras de comestibles. Estos servicios no son parte de su plan de salud y pueden cambiar en cualquier momento."
      },
      "answerText": [
        "Acceso al gimnasio, clases en vivo y entrega de comestibles.",
        "Tarifas de vuelos y hoteles con descuento.",
        "Un equipo personal de apoyo"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 incluye One Pass Select Affinity?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-08-09T00:00:00Z",
      "validEndTs": "2025-08-15T23:59:59Z"
    },
    {
      "questionExternalCode": "true_or_false_you_can_call_the_advice_line_any_time_day_or_night_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "El asesoramiento m\u00e9dico est\u00e1 disponible a cualquier hora, de d\u00eda o de noche. Puede pedir ayuda con problemas m\u00e9dicos o de salud mental siempre que lo necesite."
      },
      "answerText": [
        "FALSO",
        "VERDADERO"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "Verdadero o falso: Puede llamar a la l\u00ednea de asesoramiento en cualquier momento, de d\u00eda o de noche.",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-08-16T00:00:00Z",
      "validEndTs": "2025-08-22T23:59:59Z"
    },
    {
      "questionExternalCode": "true_or_false_kaiser_permanente_s_cost_calculator_provides_exact_costs_fo_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "La calculadora de costos de Kaiser Permanente brinda estimaciones para que tenga una idea de qu\u00e9 esperar antes de su cita."
      },
      "answerText": [
        "FALSO",
        "VERDADERO"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "Verdadero o falso: La calculadora de costos de Kaiser Permanente indica los costos exactos de todos los servicios m\u00e9dicos.",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-08-16T00:00:00Z",
      "validEndTs": "2025-08-22T23:59:59Z"
    },
    {
      "questionExternalCode": "what_condition_leads_to_yellowing_of_the_skin_and_eyes_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "La ictericia ocurre cuando el h\u00edgado no procesa bien la bilirrubina, un pigmento amarillo generado por la descomposici\u00f3n de los gl\u00f3bulos rojos. En adultos, puede indicar un problema m\u00e9dico subyacente."
      },
      "answerText": [
        "Ictericia",
        "Demasiados pl\u00e1tanos",
        "Ros\u00e1cea"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 condici\u00f3n produce coloraci\u00f3n amarillenta de la piel y los ojos?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-08-16T00:00:00Z",
      "validEndTs": "2025-08-22T23:59:59Z"
    },
    {
      "questionExternalCode": "where_can_you_find_information_on_medication_usage_side_effects_and_preca_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Con nuestra enciclopedia de medicamentos puede aprender sobre los medicamentos recetados y de venta libre: c\u00f3mo funcionan, posibles efectos secundarios y m\u00e1s."
      },
      "answerText": [
        "Wikipedia",
        "L\u00ednea directa de medicamentos de Kaiser Permanente",
        "Nuestra enciclopedia de medicamentos"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfD\u00f3nde puedo encontrar informaci\u00f3n sobre el uso de medicamentos, efectos secundarios y precauciones?",
      "correctAnswer": [
        2
      ],
      "validStartTs": "2025-08-16T00:00:00Z",
      "validEndTs": "2025-08-22T23:59:59Z"
    },
    {
      "questionExternalCode": "what_does_your_brain_do_when_you_exercise_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Las endorfinas son potenciadores naturales del estado de \u00e1nimo. El cerebro las produce cuando hace ejercicio o se r\u00ede."
      },
      "answerText": [
        "Libera endorfinas",
        "Reproduce \u201cEye of the Tiger\u201d sin parar",
        "Libera progesterona"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 hace el cerebro cuando realiza ejercicio?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-08-16T00:00:00Z",
      "validEndTs": "2025-08-22T23:59:59Z"
    },
    {
      "questionExternalCode": "what_is_a_common_contributing_cause_of_heart_disease_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "La presi\u00f3n arterial alta ejerce presi\u00f3n sobre los vasos sangu\u00edneos y, con el tiempo, puede provocar enfermedades card\u00edacas."
      },
      "answerText": [
        "La presi\u00f3n arterial alta",
        "Comer demasiada fibra",
        "Mirar demasiadas comedias rom\u00e1nticas"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfCu\u00e1l es una causa com\u00fan que contribuye a las enfermedades card\u00edacas?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-08-16T00:00:00Z",
      "validEndTs": "2025-08-22T23:59:59Z"
    },
    {
      "questionExternalCode": "what_is_the_strongest_predictor_for_developing_osteoarthritis_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "La artrosis ocurre cuando el cart\u00edlago de la articulaci\u00f3n se desgasta. Esto suele deberse a la edad o al uso excesivo, y causa dolor y rigidez."
      },
      "answerText": [
        "Fumar",
        "Envejecimiento",
        "Hacerse crujir las articulaciones como si fuera una barra luminosa"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfCu\u00e1l es el predictor m\u00e1s fuerte para el desarrollo de artrosis?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-08-16T00:00:00Z",
      "validEndTs": "2025-08-22T23:59:59Z"
    },
    {
      "questionExternalCode": "what_s_the_standard_shipping_time_for_kaiser_permanente_s_mail_order_pres_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Kaiser Permanente ofrece env\u00edo est\u00e1ndar para la mayor\u00eda de las recetas compradas por correo sin costo adicional. La entrega suele demorar entre 3 y 7 d\u00edas h\u00e1biles, seg\u00fan la ubicaci\u00f3n."
      },
      "answerText": [
        "Entre 3 y 7\u00a0d\u00edas",
        "Antes de que se le acabe el medicamento... con suerte",
        "Entre 7 y 10\u00a0d\u00edas"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfCu\u00e1l es el tiempo de env\u00edo est\u00e1ndar para las recetas m\u00e9dicas por correo de Kaiser Permanente?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-08-23T00:00:00Z",
      "validEndTs": "2025-08-29T23:59:59Z"
    },
    {
      "questionExternalCode": "what_is_credit_card_skimming_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Los estafadores utilizan dispositivos ocultos para robar la informaci\u00f3n de su tarjeta cuando la pasa. Compruebe siempre si hay algo inusual en los lectores de tarjetas."
      },
      "answerText": [
        "El robo de informaci\u00f3n de tarjetas de cajeros autom\u00e1ticos o m\u00e1quinas de pago",
        "Cuando la billetera se pone a dieta",
        "Una forma de pagar sin deslizar la tarjeta"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 es el skimming de tarjetas de cr\u00e9dito?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-08-23T00:00:00Z",
      "validEndTs": "2025-08-29T23:59:59Z"
    },
    {
      "questionExternalCode": "what_triggers_your_body_s_fight_or_flight_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "La adrenalina hace que el coraz\u00f3n bombee m\u00e1s r\u00e1pido. Esto ayuda a responder cuando est\u00e9 en peligro."
      },
      "answerText": [
        "El cortisol",
        "La adrenalina",
        "Ver la bater\u00eda del tel\u00e9fono en 1\u00a0%"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 desencadena la reacci\u00f3n de \u201clucha o huida\u201d del cuerpo?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-08-23T00:00:00Z",
      "validEndTs": "2025-08-29T23:59:59Z"
    },
    {
      "questionExternalCode": "what_sum_makes_up_the_out_of_pocket_max_for_medical_insurance_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Una vez que alcanza su m\u00e1ximo de desembolso, el seguro paga el 100\u00a0% de la mayor\u00eda de los beneficios cubiertos por el resto del per\u00edodo."
      },
      "answerText": [
        "Deducible + prima",
        "Deducible + copagos + coseguro",
        "Yo no hago matem\u00e1ticas"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 suma constituye el m\u00e1ximo de desembolso personal para el seguro m\u00e9dico?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-08-23T00:00:00Z",
      "validEndTs": "2025-08-29T23:59:59Z"
    },
    {
      "questionExternalCode": "what_do_you_need_to_submit_a_reimbursement_claim_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Aseg\u00farese de tener todos los documentos necesarios, como facturas y comprobantes de pago, para que su reclamo se procese."
      },
      "answerText": [
        "Su extracto de tarjeta de cr\u00e9dito.",
        "Una selfie en el consultorio del m\u00e9dico.",
        "Facturas detalladas, registros m\u00e9dicos y comprobantes de pago"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 necesita para presentar un reclamo de reembolso?",
      "correctAnswer": [
        2
      ],
      "validStartTs": "2025-08-23T00:00:00Z",
      "validEndTs": "2025-08-29T23:59:59Z"
    },
    {
      "questionExternalCode": "true_or_false_if_you_got_the_flu_shot_last_year_you_don_t_need_it_this_ye_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Deber\u00eda vacunarse todos los a\u00f1os. La protecci\u00f3n contra la gripe se debilita con el tiempo y la vacuna se actualiza cada a\u00f1o para proteger contra nuevos tipos del virus de la gripe."
      },
      "answerText": [
        "VERDADERO",
        "FALSO"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "Verdadero o falso: Si se vacun\u00f3 contra la gripe el a\u00f1o pasado, no necesita la vacuna este a\u00f1o.",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-08-23T00:00:00Z",
      "validEndTs": "2025-08-29T23:59:59Z"
    },
    {
      "questionExternalCode": "what_household_plant_is_poisonous_to_cats_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Los lirios son t\u00f3xicos para los gatos. Pueden causar insuficiencia renal si ingieren incluso una peque\u00f1a cantidad de la planta."
      },
      "answerText": [
        "Suculentas",
        "El que guarda rencor por haber sido atropellado",
        "Lirios"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 planta de interior es venenosa para los gatos?",
      "correctAnswer": [
        2
      ],
      "validStartTs": "2025-08-23T00:00:00Z",
      "validEndTs": "2025-08-29T23:59:59Z"
    },
    {
      "questionExternalCode": "what_should_you_do_if_you_need_urgent_or_emergency_care_while_traveling_i_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "No necesita aprobaci\u00f3n previa para atenci\u00f3n urgente o de emergencia. Quiz\u00e1 deba pagar por adelantado su atenci\u00f3n y presentar un reclamo de reembolso m\u00e1s adelante."
      },
      "answerText": [
        "Llamar a Kaiser Permanente para obtener la aprobaci\u00f3n primero.",
        "Hacer doomscrolling en las redes sociales.",
        "Ir al centro m\u00e9dico m\u00e1s cercano."
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 debe hacer si necesita atenci\u00f3n urgente o de emergencia mientras viaja internacionalmente?",
      "correctAnswer": [
        2
      ],
      "validStartTs": "2025-08-30T00:00:00Z",
      "validEndTs": "2025-09-05T23:59:59Z"
    },
    {
      "questionExternalCode": "what_s_a_common_food_in_long_living_cultures_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Los frijoles est\u00e1n repletos de fibra y prote\u00ednas, \u00a1excelentes para la longevidad!"
      },
      "answerText": [
        "Un s\u00e1ndwich submarino",
        "El pollo",
        "Los frijoles"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfCu\u00e1l es un alimento com\u00fan en las culturas longevas?",
      "correctAnswer": [
        2
      ],
      "validStartTs": "2025-08-30T00:00:00Z",
      "validEndTs": "2025-09-05T23:59:59Z"
    },
    {
      "questionExternalCode": "what_device_is_used_to_destroy_kidney_stones_with_sound_waves_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Un monitor Doppler fetal utiliza ondas sonoras para comprobar los latidos del coraz\u00f3n del beb\u00e9. Ayuda a los m\u00e9dicos a asegurarse de que el beb\u00e9 est\u00e9 sano y crezca bien."
      },
      "answerText": [
        "Un monitor Doppler",
        "Un cardiotoc\u00f3grafo",
        "Un walkie-talkie"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 dispositivo ayuda a rastrear los latidos del coraz\u00f3n de un beb\u00e9 durante el embarazo?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-08-30T00:00:00Z",
      "validEndTs": "2025-09-05T23:59:59Z"
    },
    {
      "questionExternalCode": "what_s_a_sign_of_sleep_apnea_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "La apnea del sue\u00f1o provoca ronquidos y pausas en la respiraci\u00f3n. Es importante recibir tratamiento para mejorar la calidad del sue\u00f1o."
      },
      "answerText": [
        "Disfrutar de la m\u00fasica country",
        "Cuando las ovejas empiezan a contar",
        "La somnolencia excesiva durante el d\u00eda"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 es un signo de apnea del sue\u00f1o?",
      "correctAnswer": [
        2
      ],
      "validStartTs": "2025-08-30T00:00:00Z",
      "validEndTs": "2025-09-05T23:59:59Z"
    },
    {
      "questionExternalCode": "true_or_false_the_covid_19_vaccine_and_flu_shot_make_you_sick_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "Las vacunas no causan virus, pero puede enfermarse por distintas razones. Podr\u00eda ser una enfermedad diferente, una se\u00f1al de que el cuerpo aprendi\u00f3 a combatir el virus o una exposici\u00f3n antes de recibir la vacuna."
      },
      "answerText": [
        "VERDADERO",
        "FALSO"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "Verdadero o falso: Las vacunas contra la COVID-19 y contra la gripe causan enfermedades.",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-08-30T00:00:00Z",
      "validEndTs": "2025-09-05T23:59:59Z"
    },
    {
      "questionExternalCode": "how_do_you_fill_a_prescription_while_traveling_in_a_kaiser_permanente_are_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "En el \u00e1rea de Kaiser Permanente, puede surtir sus recetas en cualquiera de nuestras farmacias. Llame a la l\u00ednea de viajes al 951-268-3900 si necesita ayuda para encontrar una.\n\nEste n\u00famero se puede marcar tanto dentro como fuera de los Estados Unidos. Antes del n\u00famero de tel\u00e9fono, marque ''001'' desde l\u00edneas fijas y ''+1'' desde celulares si se encuentra fuera del pa\u00eds. Es posible que se apliquen cargos de larga distancia y no podemos aceptar llamadas por cobrar. La l\u00ednea telef\u00f3nica permanece cerrada durante los d\u00edas festivos principales: A\u00f1o Nuevo, Pascua, D\u00eda de los Ca\u00eddos, 4 de Julio, D\u00eda del Trabajo, D\u00eda de Acci\u00f3n de Gracias y Navidad. La l\u00ednea cierra temprano, a las 10 p.\u202fm., hora del pac\u00edfico (PT), el d\u00eda anterior a un feriado y vuelve a abrir a las 4 a.\u202fm., hora del pac\u00edfico (PT) el d\u00eda posterior al feriado."
      },
      "answerText": [
        "Ofrece sus millas de viajero frecuente.",
        "En cualquier farmacia de Kaiser Permanente.",
        "Se paga por adelantado y se pide el reembolso."
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfC\u00f3mo se surte una receta m\u00e9dica mientras viaja dentro del \u00e1rea de Kaiser Permanente?",
      "correctAnswer": [
        1
      ],
      "validStartTs": "2025-08-30T00:00:00Z",
      "validEndTs": "2025-09-05T23:59:59Z"
    },
    {
      "questionExternalCode": "which_part_of_the_eye_contains_the_pigment_that_gives_it_its_color_weekly",
      "learning": {
        "header": "¿Sabías que...?",
        "description": "El iris es la parte coloreada del ojo. Tambi\u00e9n ayuda a controlar la cantidad de luz que entra al agrandar o achicar la pupila."
      },
      "answerText": [
        "El iris",
        "Mi estado de \u00e1nimo",
        "La retina"
      ],
      "answerType": "SINGLE",
      "layoutType": "BUTTON",
      "questionText": "\u00bfQu\u00e9 parte del ojo contiene el pigmento que le da su color?",
      "correctAnswer": [
        0
      ],
      "validStartTs": "2025-08-30T00:00:00Z",
      "validEndTs": "2025-09-05T23:59:59Z"
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
        v_trivia_code := gen_random_uuid();
        INSERT INTO task.trivia (
            trivia_code, task_reward_id, cta_task_external_code, config_json,
            create_ts, update_ts, create_user, update_user, delete_nbr
        ) VALUES (
            'trv-' || v_trivia_code, v_task_reward_id, v_cta_task_external_code, v_config_json::jsonb,
            now(), NULL, 'SYSTEM', NULL, 0
        );
        SELECT trivia_id INTO v_trivia_id FROM task.trivia
        WHERE trivia_code = 'trv-' || v_trivia_code;
        RAISE NOTICE '✅ Inserted trivia with ID: %', v_trivia_id;
    ELSE
        RAISE NOTICE '⚠️ Trivia already exists with ID: %', v_trivia_id;
    END IF;

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

        -- Get validity timestamps
        v_valid_start_ts := (question ->> 'validStartTs')::timestamp;
        v_valid_end_ts := (question ->> 'validEndTs')::timestamp;

        -- Check if trivia_question exists
        SELECT trivia_question_code, trivia_json
        INTO v_existing_code, v_existing_json
        FROM task.trivia_question
        WHERE question_external_code = v_question_external_code
          AND delete_nbr = 0
        LIMIT 1;

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
            IF NOT (v_existing_json ? 'es') THEN
                UPDATE task.trivia_question
                SET trivia_json = v_existing_json || jsonb_build_object('es', v_es_json),
                    update_ts = now(),
                    update_user = 'SYSTEM'
                WHERE trivia_question_code = v_existing_code;
                RAISE NOTICE '🛠 Updated trivia_question % with missing "es" block.', v_existing_code;
            ELSE
                RAISE NOTICE '⚠️ Skipped update: "es" already present for %', v_existing_code;
            END IF;
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