--Create a sample adventure, tenant_adventure and CMS component.
DO $$ 
DECLARE 
    tenant_rec RECORD; 
    comp_id BIGINT;
    comp_type_id BIGINT;
    component_code TEXT;
    component_override_code TEXT;
    adv_id BIGINT;
    adventure_code TEXT;
    tenant_adventure_code TEXT;
    component_name TEXT := 'healthy-eating'; -- Input Component Name
    adventure_name TEXT := 'Healthy Eating'; -- Input Adventure Name shown in UI
    adventure_desc TEXT := 'This adventure encourages nutritious food habits lorem ipsum'; -- Input Description
    graphic_url TEXT := 'https://app-static.integ.sunnyrewards.com/public/images/Adventure_Graphic_Image.png'; -- Input Graphic URL
    cohort_name TEXT := 'healthy-eating'; -- Input Cohort Name
BEGIN 
    FOR tenant_rec IN 
        SELECT tenant_code FROM tenant.tenant WHERE delete_nbr = 0 and tenant_code='ten-80e4b0dfcf6a4d49bf83682f364116d2'
    LOOP 
        BEGIN 
            RAISE NOTICE 'Processing tenant: %', tenant_rec.tenant_code;
			select component_type_id from cms.component_type where component_type_code='cty-9a7b2c1d4e8f4735a6c5b9d12f3e6a89' and delete_nbr = 0 into comp_type_id;
 
            -- Generate UUIDs for component and override codes
            component_code := 'com-' || REPLACE(gen_random_uuid()::text, '-', '');
            component_override_code := 'cov-' || REPLACE(gen_random_uuid()::text, '-', '');
 
            -- Insert into cms.component and retrieve component_id
            INSERT INTO cms.component (
				component_type_id,
                tenant_code, 
                component_code, 
                component_override_code, 
                component_name, 
                data_json, 
                metadata_json, 
                create_ts, 
                update_ts, 
                create_user, 
                update_user, 
                delete_nbr, 
                language_code
            ) VALUES (
				comp_type_id,
                tenant_rec.tenant_code, 
                component_code, 
                component_override_code, 
                component_name, 
                jsonb_build_object(
                    'data', jsonb_build_object(
                        'details', jsonb_build_object(
                            'name', adventure_name,
                            'graphics', jsonb_build_object('URL', graphic_url),
                            'description', adventure_desc
                        )
                    )
                ), 
                '{}', 
                NOW(), 
                NULL, 
                'SYSTEM', 
                NULL, 
                0, 
                'en-us'
            ) RETURNING component_id INTO comp_id;
 
            RAISE NOTICE 'Inserted into cms.component: component_id=%, component_code=%', comp_id, component_code;
 
            -- Generate adventure_code
            adventure_code := 'adv-' || REPLACE(gen_random_uuid()::text, '-', '');
 
            -- Insert into task.adventure and retrieve adventure_id
            INSERT INTO task.adventure (
                adventure_code, 
                adventure_config_json, 
                cms_component_code, 
                create_ts, 
                create_user, 
                update_ts, 
                update_user, 
                delete_nbr
            ) VALUES (
                adventure_code, 
                jsonb_build_object(
                    'cohorts', ARRAY['adventure:' || cohort_name]
                ), 
                component_code, 
                NOW(), 
                'SYSTEM', 
                NULL, 
                NULL, 
                0
            ) RETURNING adventure_id INTO adv_id;
 
            RAISE NOTICE 'Inserted into task.adventure: adventure_id=%, adventure_code=%', adv_id, adventure_code;
 
            -- Generate tenant_adventure_code
            tenant_adventure_code := 'tad-' || REPLACE(gen_random_uuid()::text, '-', '');
            RAISE NOTICE 'Generated tenant_adventure_code: %', tenant_adventure_code;
 
            -- Insert into task.tenant_adventure
            INSERT INTO task.tenant_adventure (
                tenant_adventure_code, 
                tenant_code, 
                adventure_id, 
                create_ts, 
                create_user, 
                update_ts, 
                update_user, 
                delete_nbr
            ) VALUES (
                tenant_adventure_code, 
                tenant_rec.tenant_code, 
                adv_id, 
                NOW(), 
                'SYSTEM', 
                NULL, 
                NULL, 
                0
            );
 
            RAISE NOTICE 'Inserted into task.tenant_adventure: tenant_adventure_code=%, adventure_id=%', tenant_adventure_code, adv_id;
 
        EXCEPTION 
            WHEN OTHERS THEN 
                RAISE WARNING 'Error occurred for tenant %: %', tenant_rec.tenant_code, SQLERRM;
        END;
 
    END LOOP;
    RAISE NOTICE 'Insertion process completed for all tenants.';
END $$;