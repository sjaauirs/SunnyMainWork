-- ===================================================================================
-- Author      : Pernati Rakesh
-- Purpose     : Insert or update predefined tenant task categories with resource JSON and audit columns
-- Jira Task   : SOCT-1595
-- ====================================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<WATCO-TENANT-CODE>'; -- 🔹 Input Tenant Code
    v_task_category_code TEXT;
    v_task_category_name TEXT;
    v_image_name TEXT;
    v_task_category_id BIGINT;
BEGIN
    BEGIN
        -- 🔹 JSON array of Task Categories
        FOR v_task_category_code, v_task_category_name, v_image_name IN
            SELECT * FROM (
                VALUES
                    ('tcc-fdb03f308cc2466ab57820833e8ed2cd', 'Preventive Care', 'Preventive-Care.png'),
                    ('tcc-10194555bfeb4545b7697df509d2637a', 'Health and Wellness', 'Health-and-Wellness.png'),
                    ('tcc-915c29cf8839465bbed54a33e6f20d57', 'Financial Wellness', 'Financial-Wellness.png'),
                    ('tcc-37ffaf47abaa4b6a84828f4dd0cba7a3', 'Company Culture', 'Company-Culture.png'),
                    ('tcc-7186dc274cc14788b64dddcc7f4f0656', 'Clinical Care Gap', 'Clinical-Care-Gap.png'),
                    ('tcc-69377045b0fe4762896c55670d04f3d7', 'Benefits', 'Benefits.png'),
                    ('tcc-6c2f5b8c44fe453a9db13fd10f7320b7', 'Behavioral Health', 'Behavioral-Health.png')
            ) AS t(task_category_code, task_category_name, image_name)
        LOOP
            BEGIN
                -- 🔹 Get Task Category ID
                SELECT task_category_id
                INTO v_task_category_id
                FROM task.task_category
                WHERE task_category_code = v_task_category_code AND delete_nbr = 0;

                IF v_task_category_id IS NULL THEN
                    RAISE WARNING '⚠️ Task Category Code "%" not found in task.task_category!', v_task_category_code;
                    CONTINUE;
                END IF;

                -- 🔹 Check if record exists
                IF EXISTS (
                    SELECT 1
                    FROM task.tenant_task_category
                    WHERE tenant_code = v_tenant_code
                      AND task_category_id = v_task_category_id
                      AND delete_nbr = 0
                ) THEN
                    -- 🔹 Update existing record
                    UPDATE task.tenant_task_category
                    SET resource_json = jsonb_build_object('taskIconUrl', '/assets/icons/' || v_image_name),
                        update_ts = NOW() AT TIME ZONE 'UTC',
                        update_user = 'SYSTEM'
                    WHERE tenant_code = v_tenant_code
                      AND task_category_id = v_task_category_id
                      AND delete_nbr = 0;

                    RAISE NOTICE '✏️ Updated existing record for Tenant="%" and Category="%" (Code=%).',
                        v_tenant_code, v_task_category_name, v_task_category_code;
                ELSE
                    -- 🔹 Insert new record
                    INSERT INTO task.tenant_task_category (
                        tenant_code,
                        task_category_id,
                        resource_json,
                        delete_nbr,
                        create_ts,
                        create_user,
                        update_ts,
                        update_user
                    )
                    VALUES (
                        v_tenant_code,
                        v_task_category_id,
                        jsonb_build_object('taskIconUrl', '/assets/icons/' || v_image_name),
                        0,
                        NOW() AT TIME ZONE 'UTC',
                        'SYSTEM',
                        NULL,
                        NULL
                    );

                    RAISE NOTICE '✅ Inserted new record for Tenant="%" and Category="%" (Code=%).',
                        v_tenant_code, v_task_category_name, v_task_category_code;
                END IF;

            EXCEPTION WHEN OTHERS THEN
                RAISE WARNING '❌ Error processing Category="%" (Code=%) for Tenant="%": %',
                    v_task_category_name, v_task_category_code, v_tenant_code, SQLERRM;
                CONTINUE;
            END;
        END LOOP;
    END;

EXCEPTION WHEN OTHERS THEN
    RAISE EXCEPTION '🚨 Fatal error in main block: %', SQLERRM;
END $$;

-- Update task category mappings
DO
$$
DECLARE
    v_user_id TEXT := 'SYSTEM';
    v_now TIMESTAMP := NOW();
    v_tenant_code TEXT := '<WATCO-TENANT-CODE>'; -- << change tenant_code here
    v_rows_updated INT;
BEGIN
    WITH mapping(task_header, task_category_name) AS (
        VALUES
            ('Attend an Open Enrollment Session', 'Benefits'),
            ('Check Out My Health Novel', 'Benefits'),
            ('Check Out the New Watco Dispatch', 'Company Culture'),
            ('Check Out Watco Team Member Discounts', 'Benefits'),
            ('Complete a Yearly Dental Exam', 'Clinical Care Gap'),
            ('Complete a Yearly Eye Exam', 'Clinical Care Gap'),
            ('Complete your 2026 Open Enrollment', 'Benefits'),
            ('Complete your Annual Wellness Physical', 'Preventive Care'),
            ('Confirm your Life Insurance Beneficiary', 'Benefits'),
            ('Learn how to use the Watco Benefit Debit Card', 'Benefits'),
            ('Download BOK App', 'Financial Wellness'),
            ('Download the BetterHelp App', 'Behavioral Health'),
            ('Download the Watco Benefitplace App', 'Benefits'),
            ('Download your Benefits Contact Card', 'Benefits'),
            ('Download Your Medical Digital ID Card', 'Benefits'),
            ('Explore an HSA', 'Financial Wellness'),
            ('Explore financial wellness tools', 'Financial Wellness'),
            ('Explore Free First Stop Health Services', 'Benefits'),
            ('Explore Lucet Resources', 'Behavioral Health'),
            ('Explore the Benefits of an FSA', 'Financial Wellness'),
            ('Explore the Tobacco Cessation Program', 'Behavioral Health'),
            ('Explore your Benefits Resources', 'Benefits'),
            ('Get a Recommended Preventive Screening', 'Preventive Care'),
            ('How to Choose a 401(k) Beneficiary', 'Benefits'),
            ('Learn about PHM - Clear Cancer', 'Benefits'),
            ('Learn how to Enroll & Access your 401(k)', 'Financial Wellness'),
            ('Make a budget', 'Financial Wellness'),
            ('Medicare Support Program', 'Benefits'),
            ('Play trivia', 'Health and Wellness'),
            ('Read the Monthly Lucet EAP Newsletter', 'Behavioral Health'),
            ('Read the Open Enrollment Guide', 'Benefits'),
            ('Review your 401(k) Savings', 'Benefits'),
            ('Review your Beneficiary with BOK', 'Benefits'),
            ('Review your Life Insurance Beneficiary', 'Benefits'),
            ('Save for Retirement', 'Financial Wellness'),
            ('Select your PCP', 'Health and Wellness'),
            ('Stay Healthy - Get Vaccinated', 'Preventive Care'),
            ('Track your Sleep Weekly', 'Health and Wellness'),
            ('Track your Steps Weekly', 'Health and Wellness'),
            ('Up Skill Yourself', 'Company Culture'),
            ('Update your Contact Information', 'Benefits'),
            ('View Your Benefit Summary', 'Benefits'),
            ('Wellness Services Covered at 100%', 'Benefits')
    )
    UPDATE task.task t
    SET task_category_id = tc.task_category_id,
        update_user = v_user_id,
        update_ts = v_now
    FROM task.task_detail td
    JOIN mapping m ON td.task_header = m.task_header AND td.delete_nbr = 0
    JOIN task.task_category tc ON tc.task_category_name = m.task_category_name AND tc.delete_nbr = 0
    WHERE t.task_id = td.task_id
      AND t.delete_nbr = 0
      AND td.tenant_code = v_tenant_code;

    -- Capture row count
    GET DIAGNOSTICS v_rows_updated = ROW_COUNT;

    RAISE NOTICE 'Bulk update complete for tenant [%]. Rows updated: %', v_tenant_code, v_rows_updated;
END
$$;