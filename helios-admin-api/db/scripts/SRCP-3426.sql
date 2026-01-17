-- Note: Before executing this script, ensure to verify if any health action already exists in the database.
-- If a health action exists, use the same task external code in the script below.
-- Otherwise, create a new health action and pass the corresponding task external code.
-- Example: const healthActionTaskExternalCode = 'test_eat_smart_for_better_health_2';
-- This script inserts a new record into the admin.script table for auto-enroll task creation, only if it does not already exist.

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
    'src-2753bd3756f146d28a9a895ab78bca9d',
    'TASK_TRIGGER',
    'This script runs when the AutoEnroll Health Action event is triggered.',
    '{
       "args": [
           { "argType": "object", "argName": "postedEventData"},
           { "argType": "Object", "argName": "autoEnrollConsumerTaskRequestDto" },
           { "argType": "Object", "argName": "autoEnrollConsumerTaskService" }
       ],
       "result": { 
                "ResultCode": "number",
                "ErrorMessage": "string", 
                "ResultMap": "Object"
                }
    }'::jsonb,
    $$
    function init(autoEnrollConsumerTaskRequestDto, autoEnrollConsumerTaskService, postedEventData) {
    // Initialize result object
    const result = {
        ResultCode: 0,
        ErrorMessage: '',
        ResultMap: {}
    };

    try {
        // Validate event type and subtype
        if (postedEventData.EventType !== 'TASK_TRIGGER' || postedEventData.EventSubtype !== 'HEALTH_TASK_PROGRESS') {
            result.ResultCode = 400; // Bad Request
            result.ErrorMessage = 'Invalid event type or subtype.';
            return result;
        }

        // Define and set the task external code in the request DTO
        const healthActionTaskExternalCode = 'test_eat_smar_for_bett_heal_2';
        autoEnrollConsumerTaskRequestDto.TaskExternalCode = healthActionTaskExternalCode;

        // Enroll the consumer task
        const response = autoEnrollConsumerTaskService.EnrollConsumerTask(autoEnrollConsumerTaskRequestDto);

        // Check if the response is valid and contains an error code
        if (!response) {
            result.ResultCode = 500; // Internal Server Error
            result.ErrorMessage = 'No response received from EnrollConsumerTask.';
            return result;
        }
		
		if (response.ErrorCode == 409) {
            result.ResultCode = 0;  // ignore 409 Error
            result.ErrorMessage = '409 : User is already enrolled in task'
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
init(autoEnrollConsumerTaskRequestDto, autoEnrollConsumerTaskService, postedEventData);
    $$,
    NOW(),          
    NULL,           
    'SYSTEM',       
    NULL,           
    0               
WHERE NOT EXISTS (
    SELECT 1 
    FROM admin.script 
    WHERE script_code = 'src-2753bd3756f146d28a9a895ab78bca9d'
);


-- This script is used to create an entry in the `admin.event_handler_script` table 
-- for handling the `TASK_TRIGGER` event type with the `HEALTH_TASK_PROGRESS` subtype.
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
     WHERE script_code = 'src-2753bd3756f146d28a9a895ab78bca9d'),
    'TASK_TRIGGER',
    'HEALTH_TASK_PROGRESS',
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
      AND event_type = 'TASK_TRIGGER'
      AND event_sub_type = 'HEALTH_TASK_PROGRESS'
);
