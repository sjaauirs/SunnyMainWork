-- ==================================================================================================
-- 🚀 Script    : Update task description for given task external code
-- 🧑 Author    : Kumar Sirikonda
-- 📅 Date      : 2025-11-18
-- 🧾 Jira      : SUN-1169,1166,1165
-- ⚠️ Inputs    : KP tenant Code (v_tenant_codes)
-- 📤 Output    : updates task.task_detail task description
-- 🔗 Script URL: 
-- 📝 Notes     : 
-- ==================================================================================================
DO $$
DECLARE
    -- 🔹 List of all tenant codes to process
    ----Input Variable
    v_tenant_codes TEXT[] := ARRAY[
        '<KP-TENANT-CODE>',
        '<KP-TENANT-CODE-QA>'
    ];
	v_mapping JSONB := '[
	{
		"task_external_code": "medi_to_boos_your_well_2026",
		"task_description": [
			{
				"type": "paragraph",
				"data": {
					"text": "Find your zen with meditation and mindfulness. Log at least 35 minutes each week and earn rewards when you clock 150 minutes in a month. That''s just 5 minutes a day! Be sure to sync your device for easier tracking."
				}
			}
		]
	},
	{
		"task_external_code": "take_a_brea_from_alco_in_marc_2026",
		"task_description": [
			{
				"type": "paragraph",
				"data": {
					"text": "Start the new year by giving your body a reset. Cutting out alcohol can improve your health, sleep, and energy. From January through March, hit pause on alcoholic drinks daily and get your monthly reward."
				}
			},
			{
				"type": "paragraph",
				"data": {
					"text": "\nTo earn your March reward, record alcohol-free days by April 10th."
				}
			}
		]
	},
	{
		"task_external_code": "step_it_up_2026",
		"task_description": [
			{
				"type": "paragraph",
				"data": {
					"text": "Step your way to better health, less stress, and more rewards. Track your steps with your preferred device or keep track manually to earn rewards when you reach 200,000 steps each month. Be sure to sync your device for easier tracking."
				}
			},
			{
				"type": "paragraph",
				"data": {
					"text": "\n*Please note that you can''t earn rewards for \"Step it up\" and \"Get moving\" activities in the same month."
				}
			}
		]
	}
]';
    -- 🔹 Loop variables
    v_tenant_code TEXT;
    v_task_id BIGINT;
    v_rowcount INT;
    item JSONB;
	
	v_task_external_code TEXT;
	v_task_description JSONB;
BEGIN
    -- 1️⃣ Loop through each tenant and process individually
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        RAISE NOTICE '🔍 Processing tenant=%', v_tenant_code;		
		 -- Loop through JSON mapping list
		FOR item IN SELECT * FROM jsonb_array_elements(v_mapping)
		LOOP
			v_task_external_code := item ->> 'task_external_code';
			v_task_description := item ->> 'task_description';
			
            RAISE NOTICE '   🔸 Checking task_external_code=%', v_task_external_code;
			-- 2️⃣ Get task_id for this tenant and task external code
			SELECT tr.task_id
			INTO v_task_id
			FROM task.task_reward tr
			WHERE tr.task_external_code = v_task_external_code
			  AND tr.delete_nbr = 0
			  AND tr.tenant_code = v_tenant_code
			LIMIT 1;

			-- 3️⃣ If task_id not found, skip this tenant
			IF v_task_id IS NULL THEN
				RAISE NOTICE '⚠️ No task_id found for tenant=% | task_external_code=%', v_tenant_code, v_task_external_code;
				CONTINUE;
			END IF;

			-- 4️⃣ Update task_detail with the new description
			UPDATE task.task_detail td
			SET task_description = v_task_description,
				update_ts = NOW(),
				update_user = 'SYSTEM'
			WHERE td.task_id = v_task_id
			  AND td.tenant_code = v_tenant_code and Language_code='en-US'
			  AND td.delete_nbr = 0;

			-- 5️⃣ Log affected rows
			GET DIAGNOSTICS v_rowcount = ROW_COUNT;

			IF v_rowcount > 0 THEN
				RAISE NOTICE '✅ Updated task_description for tenant=% | task_id=%', v_tenant_code, v_task_id;
			ELSE
				RAISE NOTICE 'ℹ️ No matching task_detail found for tenant=% | task_id=%', v_tenant_code, v_task_id;
			END IF;
		END LOOP;
    END LOOP;

    RAISE NOTICE '🎉 Update complete for all tenants (task_external_code=%).', v_task_external_code;
END $$;