-- =================================================================================================================================
-- 🚀 Script    : 1. Inserts the 'CARD_ISSUE_STATUS_UPDATE' script into admin.script (if not already present).
--   2. Creates event handler entries in admin.event_handler_script for each tenant code provided.
-- 🧑 Author    : Preeti
-- 📅 Date      : 2025-10-22
-- 🧾 Jira      : SUN-801
-- ⚠️ Inputs    : ARRAY of HAP_TENANT_CODES
-- 📤 Output    : Successfully inserted/linked CARD_ISSUE_STATUS_UPDATE for given tenant(s)
-- 🔗 Script URL: Internal QA configuration setup
-- 📝 Notes     : Execute in QA environment only. Ensure pgcrypto extension is enabled.
-- 🔢 Sequence  : 1
-- =================================================================================================================================

INSERT INTO admin.script (
    script_code, 
    script_name, 
    script_description, 
    script_json, 
    script_source, 
    create_ts, 
    update_ts, 
    create_user, 
    update_user, 
    delete_nbr
)
SELECT 
    'src-3434a648b1d748f4ac4bac7d1f9532ff',
    'CARD_ISSUE_STATUS_UPDATE',
    'This script runs when consumer accepts agreements',
    '{
       "args": [
           { "argType": "object", "argName": "agreementsVerifiedEventRequestDto"},
           { "argType": "Object", "argName": "agreementsVerifiedEventService" },
           { "argType": "Object", "argName": "postedEventData" }
       ],
       "result": { 
                "ResultCode": "number",
                "ErrorMessage": "string", 
                "ResultMap": "Object"
                }
    }'::jsonb,
    $$
   function init(agreementsVerifiedEventRequestDto, agreementsVerifiedEventService , postedEventData) {
    // Initialize result object
    const result = {
        ResultCode: 0,
        ErrorMessage: '',
        ResultMap: {}
    };

    try {
        // Validate event type and subtype
        if (postedEventData.EventType !== 'CARD_ISSUE_STATUS_UPDATE' || postedEventData.EventSubtype !== 'AGREEMENTS_VERIFIED') {
            result.ResultCode = 400; // Bad Request
            result.ErrorMessage = 'Invalid event type or subtype.';
            return result;
        }


        // Enroll the consumer task
        const response = agreementsVerifiedEventService.AgreementsVerifiedEventProcess(agreementsVerifiedEventRequestDto);

		//ignore 409 Conflict
		if (response.ErrorCode == 409) {
            result.ResultCode = 0;  // ignore 409 Error
            result.ErrorMessage = '409 -'+ response.ErrorMessage;
            return result;
			}

        if (response.ErrorCode != null) {
            result.ResultCode = response.ErrorCode;
            result.ErrorMessage = response.ErrorMessage;
            return result;
        }


        // Task enrollment successful; no further updates to the result needed.
    } catch (error) {
        // Handle unexpected errors during execution
        result.ResultCode = 1; // General error code
        result.ErrorMessage = error.message;
        result.ResultMap['StackTrace'] = error.stack; // Include stack trace for debugging
    }

    return result;
}

// Call the init function with appropriate arguments
init(agreementsVerifiedEventRequestDto, agreementsVerifiedEventService , postedEventData);
    $$,
    NOW(),          
    NULL,           
    'SYSTEM',       
    NULL,           
    0               
WHERE NOT EXISTS (
    SELECT 1 
    FROM admin.script 
    WHERE script_code = 'src-3434a648b1d748f4ac4bac7d1f9532ff'
);

DO $$
DECLARE
    v_tenant_codes      TEXT[] := ARRAY[
        'HAP-TENANT-CODE'
    ];
    v_tenant_code       TEXT;
    v_script_id         INTEGER;
    v_event_type        TEXT := 'CARD_ISSUE_STATUS_UPDATE';
    v_event_sub_type    TEXT := 'AGREEMENTS_VERIFIED';
    v_event_handler_id  INTEGER;
    v_event_handler_code TEXT;
BEGIN
    RAISE NOTICE 'Starting insert process for event handler: % / %', v_event_type, v_event_sub_type;

    -- Fetch script_id once
    SELECT script_id
    INTO v_script_id
    FROM admin.script
    WHERE script_name = v_event_type
      AND delete_nbr = 0;

    IF v_script_id IS NULL THEN
        RAISE EXCEPTION 'Script with name "%" not found in admin.script', v_event_type;
    END IF;

    -- Iterate through all tenant codes
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        RAISE NOTICE 'Processing tenant: %', v_tenant_code;

        INSERT INTO admin.event_handler_script (
            event_handler_code,
            tenant_code,
            script_id,
            event_type,
            event_sub_type,
            create_ts,
            update_ts,
            create_user,
            update_user,
            delete_nbr
        )
        SELECT
            'evh-' || replace(gen_random_uuid()::text, '-', ''),
            v_tenant_code,
            v_script_id,
            v_event_type,
            v_event_sub_type,
            now(),
            NULL,
            'IMPORT_USER',
            NULL,
            0
        WHERE NOT EXISTS (
            SELECT 1
            FROM admin.event_handler_script
            WHERE event_type = v_event_type
              AND event_sub_type = v_event_sub_type
              AND tenant_code = v_tenant_code
              AND delete_nbr = 0
        )
        RETURNING event_handler_id, event_handler_code
        INTO v_event_handler_id, v_event_handler_code;

        IF v_event_handler_id IS NOT NULL THEN
            RAISE NOTICE 'Inserted new record for tenant % -> event_handler_code: %', v_tenant_code, v_event_handler_code;
        ELSE
            RAISE NOTICE 'Record already exists for tenant %, skipping insert.', v_tenant_code;
        END IF;
    END LOOP;

    RAISE NOTICE 'Insert process completed for % tenant(s).', array_length(v_tenant_codes, 1);
END
$$;
