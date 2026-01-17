DO $$
DECLARE
    -- Inputs
    in_tenant_code       varchar(50) := '<KP-Tenant-Code>';  -- Replace with your tenant
    in_language_code     varchar(5)  := 'es'; -- Replace Language code for based on the data json conent that you are inserting
    in_base_url          text        := 'https://app-static.integ.sunnyrewards.com'; --  base URL input(possible URL for Production: https://app-static.sunnyrewards.com', UAT: https://app-static.uat.sunnyrewards.com', QA: https://app-static.qa.sunnyrewards.com', Dev: https://app-static.dev.sunnyrewards.com'

    -- Arrays
    in_component_names   text[] := ARRAY['self-care','exercise','healthy-eating'];

    in_data_json_templates text[] := ARRAY[
'{
  "data": {
    "details": {
      "name": "Cuidado personal y bienestar",
      "graphics": { "URL": "__BASE_URL__/public/images/Adventure_Graphic_Image.png" },
      "description": "Relájate, recarga energías y mejora tu bienestar físico y emocional."
    }
  }
}',
'{
  "data": {
    "details": {
      "name": "Forma física y ejercicio",
      "graphics": { "URL": "__BASE_URL__/public/images/Adventure_Graphic_Image.png" },
      "description": "Ponte en marcha con actividades que te hagan sentir en tu mejor momento."
    }
  }
}',
'{
  "data": {
    "details": {
      "name": "Alimentación saludable",
      "graphics": { "URL": "__BASE_URL__/public/images/Adventure_Graphic_Image.png" },
      "description": "Abastece tu cuerpo con comidas equilibradas para energizar tu día."
    }
  }
}'
    ];

    in_adventure_configs jsonb[] := ARRAY[
        '{"cohorts":["adventure:self-care"]}'::jsonb,
        '{"cohorts":["adventure:exercise"]}'::jsonb,
        '{"cohorts":["adventure:healthy-eating"]}'::jsonb
    ];

    -- Variables
    v_component_type_id     bigint;
    v_component_code        varchar(50);
    v_component_override    varchar(50);
    v_adventure_code        varchar(50);
    v_tenant_adventure_code varchar(50);
    v_adventure_id          bigint;
    v_existing_component_id bigint;
    v_existing_adventure_id bigint;
    v_tenant_exists         int;
    v_now                   timestamp := now();
    v_uuid                  text;
    v_data_json             jsonb;
    i                       int;
BEGIN
    RAISE NOTICE '🚀 Starting batch upsert for % components', array_length(in_component_names,1);

    -- Validate tenant
    SELECT COUNT(1) INTO v_tenant_exists
    FROM tenant.tenant
    WHERE tenant_code = in_tenant_code;

    IF v_tenant_exists = 0 THEN
        RAISE EXCEPTION '❌ Tenant % not found in tenant.tenant', in_tenant_code;
    ELSE
        RAISE NOTICE '✅ Tenant % exists', in_tenant_code;
    END IF;

    -- Fetch component_type_id
    SELECT component_type_id INTO v_component_type_id
    FROM cms.component_type
    WHERE component_type_code = 'cty-9a7b2c1d4e8f4735a6c5b9d12f3e6a89'
    LIMIT 1;

    IF v_component_type_id IS NULL THEN
        RAISE EXCEPTION '❌ component_type_code not found in cms.component_type';
    ELSE
        RAISE NOTICE '✅ component_type_id fetched: %', v_component_type_id;
    END IF;

    -- Loop through components
    FOR i IN 1..array_length(in_component_names,1) LOOP
        v_data_json := replace(in_data_json_templates[i], '__BASE_URL__', in_base_url)::jsonb;

        -- ✅ Check existing component
        SELECT component_id, component_code INTO v_existing_component_id, v_component_code
        FROM cms.component
        WHERE tenant_code = in_tenant_code
          AND component_name = in_component_names[i]
          AND language_code = in_language_code
          AND delete_nbr = 0
        LIMIT 1;

        IF v_existing_component_id IS NOT NULL THEN
            -- Update data_json
            UPDATE cms.component
               SET data_json = v_data_json,
                   update_ts = v_now,
                   update_user = 'SYSTEM'
             WHERE component_id = v_existing_component_id;

            RAISE NOTICE 'ℹ️ [%] Component exists (component_code=%), updated data_json', i, v_component_code;
        ELSE
            -- Insert new component
            v_uuid := replace(gen_random_uuid()::text, '-', '');
            v_component_code     := 'com-' || v_uuid;
            v_component_override := 'cov-' || v_uuid;

            INSERT INTO cms.component
            (
                component_type_id, tenant_code, component_code, component_override_code,
                component_name, data_json, metadata_json,
                create_ts, create_user, delete_nbr, language_code
            )
            VALUES
            (
                v_component_type_id, in_tenant_code, v_component_code, v_component_override,
                in_component_names[i], v_data_json, '{}'::jsonb,
                v_now, 'SYSTEM', 0, in_language_code
            );

            RAISE NOTICE '✅ [%] Inserted new component (component_code=%)', i, v_component_code;
        END IF;

        -- ✅ Check if adventure exists for this component
        SELECT adventure_id INTO v_existing_adventure_id
        FROM task.adventure
        WHERE cms_component_code = v_component_code
          AND delete_nbr = 0
        LIMIT 1;

        IF v_existing_adventure_id IS NOT NULL THEN
            -- Update adventure_config_json
            UPDATE task.adventure
               SET adventure_config_json = in_adventure_configs[i],
                   update_ts = v_now,
                   update_user = 'SYSTEM'
             WHERE adventure_id = v_existing_adventure_id;

            RAISE NOTICE 'ℹ️ [%] Adventure exists (adventure_id=%), updated adventure_config_json', i, v_existing_adventure_id;

            -- ✅ Check if tenant_adventure already exists
            PERFORM 1
            FROM task.tenant_adventure
            WHERE adventure_id = v_existing_adventure_id
              AND tenant_code = in_tenant_code
              AND delete_nbr = 0;

            IF NOT FOUND THEN
                v_tenant_adventure_code := 'tad-' || gen_random_uuid();
                INSERT INTO task.tenant_adventure
                (
                    tenant_adventure_code, tenant_code, adventure_id,
                    create_ts, create_user, delete_nbr
                )
                VALUES
                (
                    v_tenant_adventure_code, in_tenant_code, v_existing_adventure_id,
                    v_now, 'SYSTEM', 0
                );
                RAISE NOTICE '✅ [%] Inserted new tenant_adventure (tenant_adventure_code=%)', i, v_tenant_adventure_code;
            ELSE
                RAISE NOTICE 'ℹ️ [%] Tenant_adventure already exists for adventure_id=%', i, v_existing_adventure_id;
            END IF;

        ELSE
            -- 🚀 No adventure exists yet, insert new one
            v_adventure_code        := 'adv-' || gen_random_uuid();
            v_tenant_adventure_code := 'tad-' || gen_random_uuid();

            INSERT INTO task.adventure
            (
                adventure_code, adventure_config_json, cms_component_code,
                create_ts, create_user, delete_nbr
            )
            VALUES
            (
                v_adventure_code, in_adventure_configs[i], v_component_code,
                v_now, 'SYSTEM', 0
            )
            RETURNING adventure_id INTO v_adventure_id;

            RAISE NOTICE '✅ [%] Inserted new adventure (adventure_id=%)', i, v_adventure_id;

            INSERT INTO task.tenant_adventure
            (
                tenant_adventure_code, tenant_code, adventure_id,
                create_ts, create_user, delete_nbr
            )
            VALUES
            (
                v_tenant_adventure_code, in_tenant_code, v_adventure_id,
                v_now, 'SYSTEM', 0
            );

            RAISE NOTICE '✅ [%] Inserted new tenant_adventure (tenant_adventure_code=%)', i, v_tenant_adventure_code;
        END IF;

        RAISE NOTICE '🎯 [%] Completed processing for component_name=%', i, in_component_names[i];
    END LOOP;

    RAISE NOTICE '🎉 All % components processed successfully!', array_length(in_component_names,1);
END;
$$ LANGUAGE plpgsql;