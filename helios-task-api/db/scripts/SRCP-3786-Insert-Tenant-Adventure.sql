-- Insert script to add sample data into task.tenant_adventure table

INSERT INTO task.tenant_adventure (
    tenant_adventure_code,
    tenant_code,
    adventure_id,
    create_ts,
    create_user,
    update_ts,
    update_user,
    delete_nbr
)
VALUES
(
    'tad-' || REPLACE(gen_random_uuid()::text, '-', ''),  
    'ten-ecada21e57154928a2bb959e8365b8b4',  
    SELECT adventure_id FROM task.adventure where component_code in (SELECT component_code FROM cms.component WHERE component_name = 'mental-and-wellness' and delete_nbr=0 and tenant_code='ten-ecada21e57154928a2bb959e8365b8b4'),  
    NOW(),  
    'SYSTEM',  
    NULL, 
    NULL,  
    0  
),
(
    'tad-' || REPLACE(gen_random_uuid()::text, '-', ''),
    'ten-ecada21e57154928a2bb959e8365b8b4',
    SELECT adventure_id FROM task.adventure where component_code in (SELECT component_code FROM cms.component WHERE component_name = 'fitness-and-exercise' and delete_nbr=0 and tenant_code='ten-ecada21e57154928a2bb959e8365b8b4'),  ,
    NOW(),
    'SYSTEM',
    NULL,
    NULL,
    0
),
(
    'tad-' || REPLACE(gen_random_uuid()::text, '-', ''),
    'ten-ecada21e57154928a2bb959e8365b8b4',
    SELECT adventure_id FROM task.adventure where component_code in (SELECT component_code FROM cms.component WHERE component_name = 'healthy-eating' and delete_nbr=0 and tenant_code='ten-ecada21e57154928a2bb959e8365b8b4'),  ,
    NOW(),
    'SYSTEM',
    NULL,
    NULL,
    0
);
