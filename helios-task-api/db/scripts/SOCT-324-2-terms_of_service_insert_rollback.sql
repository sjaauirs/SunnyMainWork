/*
===============================================================================
📜 Script  : Rollback - Delete inserted Terms of Service (ToS) records
👤 Author  : [Vinod Ullaganti]
📅 Date    : [2025-05-21]
🎯 Purpose : Deletes the ToS records inserted for specific companies in both
           English and Spanish in the task.terms_of_service table.
           
🏢 Companies targeted:
   - Kaiser Permanente
   - Navitus
   - Sunny Benefits

🌐 Languages targeted:
   - en-US
   - es

🧾 Criteria for deletion:
   - delete_nbr = 1
   - terms_of_service_text contains company name
===============================================================================
📝 Notes   :
   - This rollback script removes the inserted records only, based on company
     names in the ToS text and delete_nbr = 1 flag.
   - Ensure backups before running this script.
===============================================================================
*/

DO $$
DECLARE
    company_names TEXT[] := ARRAY[
        'Kaiser Permanente',
        'Navitus',
        'Sunny Benefits'
    ];
    
    lang_codes TEXT[] := ARRAY['en-US', 'es'];
    
    company_name TEXT;
    lang TEXT;
    
    deleted_rows INT := 0;
BEGIN
    FOREACH company_name IN ARRAY company_names LOOP
        FOREACH lang IN ARRAY lang_codes LOOP
            DELETE FROM task.terms_of_service
            WHERE delete_nbr = 1
              AND LOWER(language_code) = LOWER(lang)
              AND terms_of_service_text LIKE '%' || company_name || '%';
              
            GET DIAGNOSTICS deleted_rows = ROW_COUNT;
            IF deleted_rows > 0 THEN
                RAISE NOTICE '🗑️ Deleted % row(s) for company "%", language "%".', deleted_rows, company_name, lang;
            ELSE
                RAISE NOTICE '⚠️ No rows found to delete for company "%", language "%".', company_name, lang;
            END IF;
        END LOOP;
    END LOOP;
END $$;
