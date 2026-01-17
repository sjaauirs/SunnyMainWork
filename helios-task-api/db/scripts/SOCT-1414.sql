/*
===============================================================================
 Script  : Update - task.task_detail (CTA Button Text)
 Author  : Vinod Ullaganti
 Date    : 2025-07-20
 Jira    : SOCT-1414, SOCT-1408
 Purpose : 📝 Update CTA button text for a specific task, tenant, and language
           in the task.task_detail table where the record is active.
===============================================================================
⚠️ Caution: Before Executing the Script
    ✅ Ensure the correct tenant_code, task_header, and language_code are passed
    🧪 Test in QA environment before applying in UAT or PROD
    📌 Only active records (delete_nbr = 0) will be affected
===============================================================================
*/

DO $$
DECLARE
    v_tenant_code     TEXT := '<KP TENANT_CODE>'; -- Kaiser Permanente Tenant code
    v_language_code   TEXT := 'en-US';
    v_create_user     TEXT := 'SYSTEM';
    v_i               INT;
    v_updated_count   INT := 0;

    -- Task Headers and New CTA Texts (index-aligned arrays)
    v_task_headers TEXT[] := ARRAY[
        'Play weekly trivia', -- 01
        'Play daily trivia', -- 02
        'Play healthy trivia', -- 03
		'Complete the Total Health Assessment', -- 04
		'Start your wellness coaching', -- 05
		'Get your flu vaccine', -- 06
		'Select your health adventure', -- 07
		'Step it up', -- 08 
		'Strengthen your body', -- 09
		'Track Your Sleep', -- 10
		'Meditate to boost your wellness', -- 11
        'Be mindful of what you eat', -- 12
        'Rethink your drink', -- 13
		'Try our healthy recipes', -- 14
        'Share your feedback' -- 15
    ];

    v_new_ctas TEXT[] := ARRAY[
        'Play now', -- 01
        'Play now', -- 02
        'Play now', -- 03
        'Start now', -- 04
        'Schedule now', -- 05
        'Learn more', -- 06
        'Choose now', -- 07
        'Learn more', -- 08
        'Learn more', -- 09
        'Get tips', -- 10
        'Learn more', -- 11
        'Get tips', -- 12
        'Learn more', -- 13
        'Start planning', -- 14
        'Complete now' -- 15
    ];
BEGIN
    FOR v_i IN 1..array_length(v_task_headers, 1) LOOP
        UPDATE task.task_detail
        SET
            task_cta_button_text = v_new_ctas[v_i],
            create_ts = NOW(),
            create_user = v_create_user
        WHERE tenant_code = v_tenant_code
          AND language_code = v_language_code
          AND LOWER(task_header) = LOWER(v_task_headers[v_i])
          AND delete_nbr = 0;

        GET DIAGNOSTICS v_updated_count = ROW_COUNT;

        IF v_updated_count > 0 THEN
            RAISE NOTICE '[UPDATED] CTA for "%": %', v_task_headers[v_i], v_new_ctas[v_i];
        ELSE
            RAISE WARNING '[SKIPPED] No matching record found for: "%"', v_task_headers[v_i];
        END IF;
    END LOOP;

    RAISE NOTICE '✔️ CTA button update process completed.';
END $$;