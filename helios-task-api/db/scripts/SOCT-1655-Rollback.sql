DO $$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>';  -- Replace with actual tenant code
    v_task_name TEXT := 'Get your z''s ';    -- replace the task name accordingly for QA and integ there is space at end  
    v_update_description TEXT := 
        '[{"type":"paragraph","data":{"text":"Los expertos recomiendan dormir de 7 a 9 horas cada noche para una mente y un cuerpo sanos. Usa tu dispositivo preferido u otra herramienta para registrar tu sueño. Después, registra al menos 7 horas durante al menos cuatro noches a la semana y gana recompensas mensuales al llegar a las 20 noches. \n\nPara un sueño consistente, prueba estos sencillos consejos:"}},{"type":"list","data":{"style":"unordered","items":["Establece una hora fija para acostarte y despertarte.","Acuéstate y levántate dentro de los 30 minutos posteriores a tu hora fijada para acostarte y despertarte.","Mantén tu habitación oscura y fresca."]}}]';
    v_language_code TEXT := 'es';
    v_task_detail_id BIGINT;
    v_task_id BIGINT;
BEGIN
 SELECT task_id
    INTO v_task_id
    FROM task.task
    WHERE task_name = v_task_name
      AND delete_nbr = 0
    LIMIT 1;
	
    -- Try to find task_detail_id
    SELECT task_detail_id
    INTO v_task_detail_id
    FROM task.task_detail
    WHERE task_id = v_task_id
      AND tenant_code = v_tenant_code
      AND language_code = v_language_code
      AND delete_nbr = 0
    LIMIT 1;
	
	

    -- Fail immediately if not found
    IF v_task_detail_id IS NULL THEN
        RAISE EXCEPTION 'No task_detail found for tenant: %', v_tenant_code;
    END IF;

    -- Update the matching record
    UPDATE task.task_detail
    SET task_description = v_update_description,
        update_ts = now(),
        update_user = 'SYSTEM'
    WHERE task_detail_id = v_task_detail_id;

    RAISE NOTICE 'Updated task_detail_id: %, tenant: "%"', v_task_detail_id, v_tenant_code;
END $$;