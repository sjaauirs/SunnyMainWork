/*
===============================================================================
📜 Script  : Rollback - Reset ToS reference in task_detail for specific tenants
👤 Author  : [Vinod Ullaganti]
📅 Date    : [2025-05-21]
🎯 Purpose : Resets the terms_of_service_id field to NULL in task.task_detail
           for the specified tenant codes and supported languages.
           
📝 Notes   :
   - Resets only for rows where terms_of_service_id is NOT NULL
   - Matches same tenant_code and language_code combinations as original script
   - Adjust NULL to a specific value if needed instead of NULL
===============================================================================
*/

DO $$
DECLARE
    tenant_codes TEXT[] := ARRAY[
        'ten-353ae621abde4e22be409325a1dd0eab',  -- Kaiser Permanente
        'ten-153bd6c47ebe4673a75c71faa22b9eb6',  -- Kaiser Permanente (2nd tenant)
        'ten-a468348402cd438ea9a1005ae2faedb6',  -- Navitus
        'ten-03b771f6e344406aa9603a96aca9a527'   -- Sunny Benefits
    ];
    languages TEXT[] := ARRAY['en-US', 'es'];
    
    tenant_code TEXT;
    lang TEXT;
    updated_count INT;
BEGIN
    FOREACH tenant_code IN ARRAY tenant_codes LOOP
        FOREACH lang IN ARRAY languages LOOP
            UPDATE task.task_detail
            SET terms_of_service_id = NULL
            WHERE tenant_code = tenant_code
              AND language_code = lang
              AND terms_of_service_id IS NOT NULL;
              
            GET DIAGNOSTICS updated_count = ROW_COUNT;
            
            IF updated_count > 0 THEN
                RAISE NOTICE '🗑️ Reset terms_of_service_id for % row(s) in tenant "%", language "%".', 
                    updated_count, tenant_code, lang;
            ELSE
                RAISE NOTICE '➖ No terms_of_service_id to reset for tenant "%", language "%".', tenant_code, lang;
            END IF;
        END LOOP;
    END LOOP;
END $$;
