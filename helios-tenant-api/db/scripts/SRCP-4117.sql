--update tenant_option_json
--can be one of the following values for cardIssueFlowType: 
--[ "IMMEDIATE" ] - no check, card issued at start of program
--[ "TASK_COMPLETION_CHECK" ] - only task completion check required
--[ "ELIGIBILITY_FILE_CHECK" ] - only eligibility file check required - this is for the future
--[ "TASK_COMPLETION_CHECK", "ELIGIBILITY_FILE_CHECK" ] - task completion required AND eligibility file check required - this is for the future
--taskCompletionCheckCode is set if cardIssueFlowType contains "TASK_COMPLETION_CHECK", can be one of the following values:
--[ "ANY" ] - any single task completed will be accepted as card eligible
--[ "task_reward_code_0", … ] - any one of these task rewards checked for completion will be accepted as card eligible

UPDATE tenant.tenant
SET tenant_option_json = jsonb_set(
    tenant_option_json::jsonb, 
    '{benefitsOptions}', 
    (tenant_option_json->'benefitsOptions') || 
    '{"cardIssueFlowType": ["IMMEDIATE"], "taskCompletionCheckCode": [], "manualCardRequestRequired": false}'::jsonb
)
WHERE tenant_option_json IS NOT NULL
	AND tenant_option_json <> '{}'::jsonb 
	AND tenant_option_json::jsonb ? 'benefitsOptions';
