--Rollback 
UPDATE tenant.tenant
SET tenant_option_json = jsonb_set(
    tenant_option_json::jsonb, 
    '{benefitsOptions}', 
    (tenant_option_json->'benefitsOptions') - 'cardIssueFlowType' - 'taskCompletionCheckCode' - 'manualCardRequestRequired'
)
WHERE  tenant_option_json IS NOT NULL
	AND tenant_option_json <> '{}'::jsonb 
	AND tenant_option_json::jsonb ? 'benefitsOptions' 
