/*
===============================================================================
📜 Script  : Update ToS Reference in task_detail for Specific Tenants
👤 Author  : [Vinod Ullaganti]
📅 Date    : [2025-05-19]
📄 Purpose : 
   For each specified tenant_code and company_name, fetch the matching 
   ToS entries in both supported languages and update corresponding 
   task_detail records (filtered by tenant_code only).

🌐 Languages Supported:
   - en-US
   - es
===============================================================================
📝 Notes   :
   - 🔍 Uses partial match on company_name to locate ToS entries.
   - 🎯 Only updates task_detail rows for the specified tenant_code.
   - 🚫 Skips updates where terms_of_service_id is already correct.
   - 📌 Target Table: task.task_detail
===============================================================================
*/

-- 🏢 For Tenant: Kaiser Permanente (Code: ten-353ae621abde4e22be409325a1dd0eab)
DO $$
DECLARE
    v_tenant_code TEXT := 'ten-353ae621abde4e22be409325a1dd0eab';
    v_company_name TEXT := 'Kaiser Permanente';
    languages TEXT[] := ARRAY['en-US', 'es'];
    current_lang TEXT;
    tos_id INT;
    updated_count INT;
BEGIN
    FOREACH current_lang IN ARRAY languages LOOP
        SELECT tos.terms_of_service_id INTO tos_id
        FROM task.terms_of_service tos
        WHERE tos.language_code = current_lang
          AND tos.terms_of_service_text ILIKE '%' || v_company_name || '%'
        LIMIT 1;

        IF tos_id IS NOT NULL THEN
            UPDATE task.task_detail td
            SET terms_of_service_id = tos_id
            WHERE td.tenant_code = v_tenant_code
              AND td.language_code = current_lang
              AND (td.terms_of_service_id IS DISTINCT FROM tos_id);

            GET DIAGNOSTICS updated_count = ROW_COUNT;

            IF updated_count > 0 THEN
                RAISE NOTICE '✔ Updated % row(s) in task_detail for tenant "%", company "%", language "%"', 
                    updated_count, v_tenant_code, v_company_name, current_lang;
            ELSE
                RAISE NOTICE '➖ No updates needed in task_detail for tenant "%", company "%", language "%"', 
                    v_tenant_code, v_company_name, current_lang;
            END IF;
        ELSE
            RAISE NOTICE '✖ No ToS found for company "%", language "%"', 
                v_company_name, current_lang;
        END IF;
    END LOOP;
END $$;

-- 🏢 For Tenant: Kaiser Permanente (Code: ten-153bd6c47ebe4673a75c71faa22b9eb6)
DO $$
DECLARE
    v_tenant_code TEXT := 'ten-153bd6c47ebe4673a75c71faa22b9eb6';
    v_company_name TEXT := 'Kaiser Permanente';
    languages TEXT[] := ARRAY['en-US', 'es'];
    current_lang TEXT;
    tos_id INT;
    updated_count INT;
BEGIN
    FOREACH current_lang IN ARRAY languages LOOP
        SELECT tos.terms_of_service_id INTO tos_id
        FROM task.terms_of_service tos
        WHERE tos.language_code = current_lang
          AND tos.terms_of_service_text ILIKE '%' || v_company_name || '%'
        LIMIT 1;

        IF tos_id IS NOT NULL THEN
            UPDATE task.task_detail td
            SET terms_of_service_id = tos_id
            WHERE td.tenant_code = v_tenant_code
              AND td.language_code = current_lang
              AND (td.terms_of_service_id IS DISTINCT FROM tos_id);

            GET DIAGNOSTICS updated_count = ROW_COUNT;

            IF updated_count > 0 THEN
                RAISE NOTICE '✔ Updated % row(s) in task_detail for tenant "%", company "%", language "%"', 
                    updated_count, v_tenant_code, v_company_name, current_lang;
            ELSE
                RAISE NOTICE '➖ No updates needed in task_detail for tenant "%", company "%", language "%"', 
                    v_tenant_code, v_company_name, current_lang;
            END IF;
        ELSE
            RAISE NOTICE '✖ No ToS found for company "%", language "%"', 
                v_company_name, current_lang;
        END IF;
    END LOOP;
END $$;

-- 🏢 For Tenant: Navitus (Code: ten-a468348402cd438ea9a1005ae2faedb6)
DO $$
DECLARE
    v_tenant_code TEXT := 'ten-a468348402cd438ea9a1005ae2faedb6';
    v_company_name TEXT := 'Navitus';
    languages TEXT[] := ARRAY['en-US', 'es'];
    current_lang TEXT;
    tos_id INT;
    updated_count INT;
BEGIN
    FOREACH current_lang IN ARRAY languages LOOP
        SELECT tos.terms_of_service_id INTO tos_id
        FROM task.terms_of_service tos
        WHERE tos.language_code = current_lang
          AND tos.terms_of_service_text ILIKE '%' || v_company_name || '%'
        LIMIT 1;

        IF tos_id IS NOT NULL THEN
            UPDATE task.task_detail td
            SET terms_of_service_id = tos_id
            WHERE td.tenant_code = v_tenant_code
              AND td.language_code = current_lang
              AND (td.terms_of_service_id IS DISTINCT FROM tos_id);

            GET DIAGNOSTICS updated_count = ROW_COUNT;

            IF updated_count > 0 THEN
                RAISE NOTICE '✔ Updated % row(s) in task_detail for tenant "%", company "%", language "%"', 
                    updated_count, v_tenant_code, v_company_name, current_lang;
            ELSE
                RAISE NOTICE '➖ No updates needed in task_detail for tenant "%", company "%", language "%"', 
                    v_tenant_code, v_company_name, current_lang;
            END IF;
        ELSE
            RAISE NOTICE '✖ No ToS found for company "%", language "%"', 
                v_company_name, current_lang;
        END IF;
    END LOOP;
END $$;

-- 🏢 For Tenant: Sunny Employee Benefits Program 2025 (Code: ten-03b771f6e344406aa9603a96aca9a527)
DO $$
DECLARE
    v_tenant_code TEXT := 'ten-03b771f6e344406aa9603a96aca9a527';
    v_company_name TEXT := 'Sunny Benefits';
    languages TEXT[] := ARRAY['en-US', 'es'];
    current_lang TEXT;
    tos_id INT;
    updated_count INT;
BEGIN
    FOREACH current_lang IN ARRAY languages LOOP
        SELECT tos.terms_of_service_id INTO tos_id
        FROM task.terms_of_service tos
        WHERE tos.language_code = current_lang
          AND tos.terms_of_service_text ILIKE '%' || v_company_name || '%'
        LIMIT 1;

        IF tos_id IS NOT NULL THEN
            UPDATE task.task_detail td
            SET terms_of_service_id = tos_id
            WHERE td.tenant_code = v_tenant_code
              AND td.language_code = current_lang
              AND (td.terms_of_service_id IS DISTINCT FROM tos_id);

            GET DIAGNOSTICS updated_count = ROW_COUNT;

            IF updated_count > 0 THEN
                RAISE NOTICE '✔ Updated % row(s) in task_detail for tenant "%", company "%", language "%"', 
                    updated_count, v_tenant_code, v_company_name, current_lang;
            ELSE
                RAISE NOTICE '➖ No updates needed in task_detail for tenant "%", company "%", language "%"', 
                    v_tenant_code, v_company_name, current_lang;
            END IF;
        ELSE
            RAISE NOTICE '✖ No ToS found for company "%", language "%"', 
                v_company_name, current_lang;
        END IF;
    END LOOP;
END $$;
