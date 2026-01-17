-- This query will softdeletes the Existed cohort_tenant_task_reward which are mapped with adventure cohorts 
UPDATE cohort.cohort_tenant_task_reward ctt
SET delete_nbr = ctt.cohort_tenant_task_reward_id
WHERE ctt.task_reward_code IN (
    SELECT tr.task_reward_code
    FROM task.task_reward tr
    INNER JOIN cohort.cohort_tenant_task_reward ctt_inner 
        ON tr.task_reward_code = ctt_inner.task_reward_code
    INNER JOIN cohort.cohort c 
        ON ctt_inner.cohort_id = c.cohort_id
    WHERE c.cohort_name ILIKE '%adventure%' 
        AND c.delete_nbr = 0
        AND ctt_inner.tenant_code = 'ten-ecada21e57154928a2bb959e8365b8b4'
        AND ctt_inner.delete_nbr = 0
        AND tr.tenant_code = 'ten-ecada21e57154928a2bb959e8365b8b4'
        AND tr.delete_nbr = 0
        AND tr.is_collection = FALSE
);