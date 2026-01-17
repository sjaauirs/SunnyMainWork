-- This Script is Specific to UAT And QA only

-- SOCT-1601: This script updates the task_description in task.task_detail TABLE
-- Replace the input parameters before executing the script

DO $$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>';   -- Replace with actual tenant code 
    v_task_header TEXT := 'Duerma mejor';   

    -- Spanish description
    v_update_description_es TEXT := 
        '[{"type":"paragraph","data":{"text":"Los expertos recomiendan dormir de 7 a 9 horas cada noche para una mente y un cuerpo sanos. Usa tu dispositivo preferido u otra herramienta para registrar tu sueño. Después, registra al menos 7 horas durante al menos cuatro noches a la semana y gana recompensas mensuales al llegar a las 20 noches. \n\nPara un sueño consistente, prueba estos sencillos consejos:"}},{"type":"list","data":{"style":"unordered","items":["Establece una hora fija para acostarte y despertarte.","Acuéstate y levántate dentro de los 30 minutos posteriores a tu hora fijada para acostarte y despertarte.","Mantén tu habitación oscura y fresca."]}}]';

    v_task_detail_id BIGINT;
BEGIN
 -- ================= SPANISH UPDATE =================
    SELECT task_detail_id
    INTO v_task_detail_id
    FROM task.task_detail
    WHERE task_header = v_task_header
      AND tenant_code = v_tenant_code
      AND language_code = 'es'
      AND delete_nbr = 0
    LIMIT 1;

    IF v_task_detail_id IS NULL THEN
        RAISE EXCEPTION 'No Spanish task_detail found for tenant: %, header: "%"', v_tenant_code, v_task_header;
    END IF;

    UPDATE task.task_detail
    SET task_description = v_update_description_es,
        update_ts = now(),
        update_user = 'SYSTEM'
    WHERE task_detail_id = v_task_detail_id;

    RAISE NOTICE 'Updated SPANISH task_detail_id: %, header: "%", tenant: "%"', v_task_detail_id, v_task_header, v_tenant_code;
END $$;
