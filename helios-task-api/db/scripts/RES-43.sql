DO $$
DECLARE
    v_tenant_code TEXT := '<tenant_code_KP>'; -- Replace with actual tenant code
    v_mapping JSONB;
BEGIN
    -- ✅ Full v_mapping JSON
    v_mapping := jsonb '[
      {
        "task_external_code": "volu_your_time_2026",
        "criteria_json": {
          "imageCriteria": {
            "unitLabel": {
              "es": "fotos",
              "en-US": "photos"
            },
            "buttonLabel": {
              "es": "Añadir foto",
              "en-US": "Add photo"
            },
            "requiredImageCount": 1,
            "icon": { "modalIconUrl": null },
            "imageCriteriaText": {
              "en-US": [
                { "type": "header", "data": { "text": "Show how you give back" }},
                { "type": "paragraph", "data": { "text": "Upload a photo of your volunteer work this month." }}
              ],
               "es": [
                { "type": "header", "data": { "text": "Muestre su contribución" }},
                { "type": "paragraph", "data": { "text": "Suba una foto de su voluntariado de este mes." }}
              ]
            },
            "imageCriteriaTextAlignment": "left"
          },
          "completionCriteriaType": "IMAGE"
        }
      }
    ]';

    -- ✅ Set-based UPDATE using jsonb_to_recordset
    UPDATE task.task_reward t
    SET task_completion_criteria_json = m.criteria_json
    FROM jsonb_to_recordset(v_mapping) AS m(
        task_external_code TEXT,
        criteria_json JSONB
    )
    WHERE t.task_external_code = m.task_external_code
      AND t.tenant_code = v_tenant_code
      AND t.delete_nbr = 0;

    RAISE NOTICE 'Update completed';
END $$;
