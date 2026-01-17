
-- This script inserts a new record into the admin.script table for initial funding
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
    'src-2827d987b0d1427a8c74921f591e83f6',
    'PICK_A_PURSE',
    'This script runs when the Pick a Purse event is triggered.',
    '{
       "args": [
           { "argType": "Object", "argName": "initialFundingRequestDto" },
           { "argType": "Object", "argName": "onBoardingInitialFundingService" }
       ],
       "result": { 
                "ResultCode": "number",
                "ErrorMessage": "string", 
                "ResultMap": "Object"
                }
    }'::jsonb,
    $$
    function init(initialFundingRequestDto, onBoardingInitialFundingService) {
    // Initialize result object
    const result = {
        ResultCode: 0,
        ErrorMessage: '',
        ResultMap: {}
    };

    try {
        // Processes the initial funding
        const response = onBoardingInitialFundingService.ProcessInitialFundingAsync(initialFundingRequestDto);

        // Check if the response is valid and contains an error code
        if (!response) {
            result.ResultCode = 500; // Internal Server Error
            result.ErrorMessage = 'No response received from EnrollConsumerTask.';
            return result;
        }

        if (response.ErrorCode != null) {
            result.ResultCode = response.ErrorCode;
            result.ErrorMessage = response.ErrorMessage;
            return result;
        }
    } catch (error) {
        // Handle unexpected errors during execution
        result.ResultCode = 1; // General error code
        result.ErrorMessage = error.message;
        result.ResultMap['StackTrace'] = error.stack; // Include stack trace for debugging
    }

    return result;
}

// Call the init function with appropriate arguments
init(initialFundingRequestDto, onBoardingInitialFundingService);
    $$,
    NOW(),          
    NULL,           
    'SYSTEM',       
    NULL,           
    0               
WHERE NOT EXISTS (
    SELECT 1 
    FROM admin.script 
    WHERE script_code = 'src-2827d987b0d1427a8c74921f591e83f6'
);


-- This script is used to create an entry in the `admin.event_handler_script` table 
-- for handling the `PICK_A_PURSE` event type with the `NONE` subtype.
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
     WHERE script_code = 'src-2827d987b0d1427a8c74921f591e83f6'),
    'PICK_A_PURSE',
    'NONE',
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
      AND event_type = 'PICK_A_PURSE'
      AND event_sub_type = 'NONE'
);
