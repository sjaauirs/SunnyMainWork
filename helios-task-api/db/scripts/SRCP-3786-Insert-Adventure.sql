-- Insert script to add sample data into task.adventure table

INSERT INTO task.adventure (
    adventure_code,
    adventure_config_json,
    cms_component_code,
    create_ts,
    create_user,
    update_ts,
    update_user,
    delete_nbr
)
VALUES
(
    'adv-' || REPLACE(gen_random_uuid()::text, '-', ''), -- Generates a GUID without hyphens
    '{"cohorts": ["adventure:abcd"]}', -- JSON config
    (SELECT component_code FROM cms.component WHERE component_name = 'mental-and-wellness' and delete_nbr=0 and tenant_code='ten-ecada21e57154928a2bb959e8365b8b4'), -- Fetches cms_component_code
    NOW(),
    'SYSTEM',
    null,
    null,
    0
),
(
    'adv-' || REPLACE(gen_random_uuid()::text, '-', ''),
    '{"cohorts": ["adventure:abcd"]}',
    (SELECT component_code FROM cms.component WHERE component_name = 'fitness-and-exercise' and delete_nbr=0 and tenant_code='ten-ecada21e57154928a2bb959e8365b8b4'),
    NOW(),
    'SYSTEM',
    null,
    null,
    0
),
(
    'adv-' || REPLACE(gen_random_uuid()::text, '-', ''),
    '{"cohorts": ["adventure:abcd"]}',
    (SELECT component_code FROM cms.component WHERE component_name = 'healthy-eating' and delete_nbr=0 and tenant_code='ten-ecada21e57154928a2bb959e8365b8b4'),
    NOW(),
    'SYSTEM',
    null,
	null,
    0
);
