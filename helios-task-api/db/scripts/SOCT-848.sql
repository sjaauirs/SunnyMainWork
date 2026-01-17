-- Update STEP tasks: Add selfReportType = "INPUT" for applicable task_reward entries
UPDATE task.task_reward
SET task_completion_criteria_json = jsonb_set(
    task_completion_criteria_json,
    '{selfReportType}',
    '"INPUT"',  
    true        
)
WHERE
    task_completion_criteria_json ->> 'completionCriteriaType' = 'HEALTH' AND
    task_completion_criteria_json -> 'healthCriteria' ->> 'healthTaskType' = 'STEPS';

-- Update SLEEP tasks: Add selfReportType = "INTERACTIVE" for applicable task_reward entries
UPDATE task.task_reward
SET task_completion_criteria_json = jsonb_set(
    task_completion_criteria_json,
    '{selfReportType}',
    '"INTERACTIVE"',  
    true
)
WHERE
    task_completion_criteria_json ->> 'completionCriteriaType' = 'HEALTH' AND
    task_completion_criteria_json -> 'healthCriteria' ->> 'healthTaskType' = 'SLEEP';
