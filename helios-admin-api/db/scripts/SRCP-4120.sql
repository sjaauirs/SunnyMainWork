
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
    'src-266c482017564f15960c11dbea1ae40b',
    'CONSUMER_TASK',
    'This script runs when Consumer task Event is received.',
    '{
       "args": [
           { "argType": "object", "argName": "postedEventData"},
           { "argType": "Object", "argName": "consumerTaskEventRequestDto" },
           { "argType": "Object", "argName": "consumerTaskEventService" }
       ],
       "result": { 
                "ResultCode": "number",
                "ErrorMessage": "string", 
                "ResultMap": "Object"
                }
    }'::jsonb,
    $$
   function init(consumerTaskEventRequestDto, consumerTaskEventService , postedEventData) {
    // Initialize result object
    const result = {
        ResultCode: 0,
        ErrorMessage: '',
        ResultMap: {}
    };

    try {
        // Validate event type and subtype
        if (postedEventData.EventType !== 'CONSUMER_TASK' || postedEventData.EventSubtype !== 'CONSUMER_TASK_UPDATE') {
            result.ResultCode = 400; // Bad Request
            result.ErrorMessage = 'Invalid event type or subtype.';
            return result;
        }


        // Enroll the consumer task
        const response = consumerTaskEventService.ConsumerTaskEventProcess(consumerTaskEventRequestDto);

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
init(consumerTaskEventRequestDto, consumerTaskEventService, postedEventData);
    $$,
    NOW(),          
    NULL,           
    'SYSTEM',       
    NULL,           
    0               
WHERE NOT EXISTS (
    SELECT 1 
    FROM admin.script 
    WHERE script_code = 'src-266c482017564f15960c11dbea1ae40b'
);


-- This script is used to create an entry in the `admin.event_handler_script` table 
-- for handling the `CONSUMER-TASK` event type with the `CONAUMER-TASk-UPDATE` subtype.
WITH tenant_codes AS (
    SELECT tenant_code 
    FROM tenant.tenant 
    WHERE delete_nbr = 0
)
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
    'evh-' || REPLACE(CAST(gen_random_uuid() as varchar), '-', ''),
    tenant_code,
    (SELECT script_id 
     FROM admin.script 
     WHERE script_code = 'src-266c482017564f15960c11dbea1ae40b'),
    'CONSUMER_TASK',
    'CONSUMER_TASK_UPDATE',
    NOW(),
    NULL,
    'SYSTEM',
    NULL,
    0
FROM tenant_codes
WHERE NOT EXISTS (
    SELECT 1
    FROM admin.event_handler_script
    WHERE tenant_code = tenant_codes.tenant_code
      AND event_type = 'CONSUMER_TASK'
      AND event_sub_type = 'CONSUMER_TASK_UPDATE'
);
