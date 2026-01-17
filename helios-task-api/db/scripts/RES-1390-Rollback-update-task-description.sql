--===============================================================================
-- Script Name : Rollback Task Descriptions by Tenant
-- Description : Updates task_detail.task_description for specific tasks
--               across one or more tenants using predefined JSON mappings.               
-- Details     :
--   - Iterates through a list of tenant codes
--   - Matches tasks using task_external_code
--   - Updates English (en-US) task descriptions only
--   - Skips deleted records (delete_nbr = 0)
--
-- Author      : Kumar Sirikonda
-- Jira        : RES-1390
--===============================================================================
DO $$
DECLARE
    -- Input array of tenant codes
    v_tenant_codes TEXT[] := ARRAY['<HAP-TENANT-CODE>', '<HAP-TENANT-CODE>'];
  v_mapping JSONB := '[
        {
            "taskExternalCode": "comp_your_annu_well_visi",
            "taskDescription": [
				{
					"type": "paragraph",
					"data": {
						"text": "An Annual Wellness Visit is a check-in with your doctor to help you stay healthy. It''s not the same as a yearly physical, but you can do both in one visit. During your wellness visit, your doctor will:\n\n"
					}
				},
				{
					"type": "list",
					"data": {
						"style": "unordered",
						"items": [
							"Talk about your health and family history",
							"Plan any needed screenings",
							"Check your thinking and memory",
							"Go over your current doctors and prescriptions"
						]
					}
				},
				{
					"type": "paragraph",
					"data": {
						"text": "\nMake your appointment today. After we confirm your visit, your reward will show up. This can take a couple weeks. Need help finding a doctor? Use the \"Find a Doctor\" tool."
					}
				}
			]
        },
        {
           "taskExternalCode": "get_your_flu_vacc",
            "taskDescription": [
				{
					"type": "paragraph",
					"data": {
						"text": "Flu season starts in October. Get your annual flu shot to avoid getting really sick or going to the hospital. It also helps protect your family and friends.\n\nGet your shot to stay healthy and earn a reward!"
					}
				}
			]
        },
        {
            "taskExternalCode": "comp_your_a1c_test",
            "taskDescription": [
				{
					"type": "paragraph",
					"data": {
						"text": "Managing your diabetes means having your A1C checked regularly to avoid complications.\n\nGet one A1C test between January 1 and June 30 to earn a reward. Get another between July 1 and December 31 to earn a second reward.\n\nCall your doctor today to schedule your next A1C test."
					}
				}
			]
        },
        {
            "taskExternalCode": "comp_your_diab_eye_exam",
            "taskDescription": [
				{
					"type": "paragraph",
					"data": {
						"text": "Diabetes can affect your eyes, so regular eye exams are important to avoid vision loss.\n\nCall your eye doctor to schedule a diabetic eye exam. Once you complete your exam, you''ll earn a reward."
					}
				}
			]
        },
        {
           "taskExternalCode": "comp_a_reco_colo_scre",
            "taskDescription": [
				{
					"type": "paragraph",
					"data": {
						"text": "Screenings can help find colon cancer early, when it''s easier to treat. Your doctor may suggest one of these:\n\nColonoscopy: checks your whole colon (usually every 10 years)\n\nSigmoidoscopy: checks the lower part of your colon and\nrectum (usually every 5 years)\n"
					}
				},
				{
					"type": "paragraph",
					"data": {
						"text": "\nTalk to your doctor to see if you need a screening and to schedule it. You''ll earn a reward once you complete one."
					}
				}
			]
        },
        {
            "taskExternalCode": "comp_your_brea_canc_scre",
            "taskDescription": [
				{
					"type": "paragraph",
					"data": {
						"text": "Mammograms are the best way to find breast cancer early. Finding it early means a better chance of beating it.\n\nTalk with your doctor to see if you need a screening. If you are due for a screening, get it done and earn a reward."
					}
				}
			]
        }
    ]';


	item JSONB;
    v_task_external_code TEXT;
    v_task_description TEXT;
    v_tenant_code TEXT;

    v_task_id BIGINT;
    v_lang TEXT := 'en-US';
    v_create_user TEXT := 'SYSTEM';  
    rows_updated INT;
BEGIN
	-- Loop tenants
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        RAISE NOTICE 'Processing tenant: %', v_tenant_code;
		-- Loop through each item in the JSON array
		FOR item IN SELECT * FROM jsonb_array_elements(v_mapping)
		LOOP
			-- Extract fields from the JSON object
			v_task_external_code := item ->> 'taskExternalCode';
			v_task_description   := (item -> 'taskDescription')::text;  -- cast JSON array to text

			RAISE NOTICE 'Processing taskExternalCode: % for tenant: %', v_task_external_code, v_tenant_code;
		   
			-- Get matching task_ids for the given tenant and external code
			FOR v_task_id IN
				SELECT task_id
				FROM task.task_reward
				WHERE tenant_code = v_tenant_code
				  AND task_external_code = v_task_external_code
				  AND delete_nbr = 0
			LOOP
				RAISE NOTICE 'Found task_id: % for taskExternalCode: %', v_task_id, v_task_external_code;

				-- Try update first
				UPDATE task.task_detail
				SET task_description   = v_task_description,
					update_user        = v_create_user,
					update_ts          = NOW()
				WHERE task_id = v_task_id
				  AND language_code = v_lang
				  AND tenant_code = v_tenant_code and delete_nbr=0;
				
				RAISE NOTICE 'Updated task_detail for task_id: %', v_task_id;
			END LOOP;
		END LOOP;
    END LOOP;
END $$;