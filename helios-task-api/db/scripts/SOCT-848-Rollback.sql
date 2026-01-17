-- Rollback: Remove selfReportType for STEPS
UPDATE task.task_reward
SET task_completion_criteria_json = task_completion_criteria_json - 'selfReportType'
WHERE
    self_report = true
    AND task_completion_criteria_json ->> 'completionCriteriaType' = 'HEALTH'
    AND task_completion_criteria_json -> 'healthCriteria' ->> 'healthTaskType' = 'STEPS'
    AND task_completion_criteria_json ? 'selfReportType';

-- Rollback: Remove selfReportType for SLEEP
UPDATE task.task_reward
SET task_completion_criteria_json = task_completion_criteria_json - 'selfReportType'
WHERE
    self_report = true
    AND task_completion_criteria_json ->> 'completionCriteriaType' = 'HEALTH'
    AND task_completion_criteria_json -> 'healthCriteria' ->> 'healthTaskType' = 'SLEEP'
    AND task_completion_criteria_json ? 'selfReportType';
