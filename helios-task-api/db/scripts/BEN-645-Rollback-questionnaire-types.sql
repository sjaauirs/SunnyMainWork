-- ============================================================================
-- 🔄 Rollback Script: Remove Task Types (SURVEY, FEEDBACK)
-- 📌 Purpose: Deletes(soft-delete) the seeded task types if they exist
-- ============================================================================

DO $$
BEGIN
    UPDATE task.task_type
    SET delete_nbr = task_type_id
    WHERE task_type_code IN ('tty-86398dc3a77d4a3db7922e57b5b6d73c', 'tty-9fcbfa97e2e24e37910dd69d3370eded')
      AND delete_nbr = 0;

    RAISE NOTICE 'Soft deleted Task Types: SURVEY, FEEDBACK (delete_nbr updated to task_type_id if existed)';
END
$$;

