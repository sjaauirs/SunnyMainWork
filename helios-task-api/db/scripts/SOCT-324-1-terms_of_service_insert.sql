/*
===============================================================================
📜 Script  : Insert Terms of Service (ToS) records for selected companies
👤 Author  : [Vinod Ullaganti]
📅 Date    : [2025-05-19]
🎯 Purpose : Inserts Terms of Service text into the task.terms_of_service table 
           for a list of predefined tenant companies in both English and Spanish.

🏢 Companies (Tenants) include:
   - Kaiser Permanente
   - Navitus
   - Sunny Benefits

🌐 Languages supported:
   - en-US
   - es

🧾 Properties inserted:
   - terms_of_service_text (localized per company & language)
   - language_code
   - create_ts
   - create_user
   - delete_nbr = 0
===============================================================================
📝 Notes   :
   - Ensures duplicates are not inserted by checking if similar text already 
     exists in the table for each company/language combination.
   - Uses string templates and dynamic formatting for localized ToS content.
   - Provides a NOTICE log for each insert or skip action.
===============================================================================
*/

DO $$
DECLARE
    -- 👤 User performing the insert
    v_create_user TEXT := 'per-915325069cdb42c783dd4601e1d27704';

    -- 🏢 Company names (tenants) for which ToS needs to be inserted
    company_names TEXT[] := ARRAY[
        'Kaiser Permanente',
        'Navitus',
        'Sunny Benefits'
    ];

    -- 🌐 Supported languages
    languages TEXT[] := ARRAY['en-US', 'es'];

    -- 🔁 Loop variables
    company_name TEXT;
    lang TEXT;

    -- 📜 ToS text templates
    tos_text_template_en TEXT := 
        'We provide you access and use of our websites, including and other Internet sites, mobile applications, and social media sites operated by or for %s (collectively, the ''Sites''), subject to your compliance with these terms and conditions of use (the ''Site Terms''). By accessing, browsing, and using the Sites, you agree to be bound by the Site Terms and all applicable law. If you do not agree to be bound by the Site Terms and applicable law each time you use the Sites or you do not have the authority to agree to or accept these Site Terms, you may not use the Sites.';

    tos_text_template_es TEXT := 
        'Le proporcionamos acceso y uso a nuestros sitios web, incluyendo otros sitios de internet, aplicaciones móviles y redes sociales operados por o para %s (en conjunto, los «Sitios»), sujeto al cumplimiento de estos términos y condiciones de uso (los «Términos del Sitio»). Al acceder, navegar y usar los Sitios, usted acepta estar sujeto a los Términos del Sitio y a toda la legislación aplicable. Si no acepta estar sujeto a los Términos del Sitio y a la legislación aplicable cada vez que usa los Sitios, o si no tiene la autoridad para aceptar estos Términos del Sitio, no podrá usarlos.';

    -- 📄 Final ToS content per company & language
    final_text TEXT;

BEGIN
    -- 🔁 Loop through each company
    FOREACH company_name IN ARRAY company_names LOOP
        -- 🌐 Loop through each language
        FOREACH lang IN ARRAY languages LOOP
            -- 🧠 Format localized ToS text
            IF lang = 'es' THEN
                final_text := format(tos_text_template_es, company_name);
            ELSE
                final_text := format(tos_text_template_en, company_name);
            END IF;

            -- 🔎 Check for existing ToS to avoid duplication
            IF NOT EXISTS (
                SELECT 1 
                FROM task.terms_of_service
                WHERE LOWER(language_code) = LOWER(lang)
                  AND terms_of_service_text LIKE '%' || company_name || '%'
            ) THEN
                -- ✅ Insert new ToS record
                INSERT INTO task.terms_of_service (
                    terms_of_service_text,
                    language_code,
                    create_ts,
                    create_user,
                    delete_nbr
                )
                VALUES (
                    final_text,
                    lang,
                    NOW(),
                    v_create_user,
                    0
                );
                RAISE NOTICE '✅ Inserted ToS for company "%", language "%"', company_name, lang;
            ELSE
                -- ⚠️ Skip if already present
                RAISE NOTICE '⚠️ Skipped existing ToS for company "%", language "%"', company_name, lang;
            END IF;
        END LOOP;
    END LOOP;
END $$;
