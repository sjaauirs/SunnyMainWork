-- ============================================================================
-- Author      : Siva Krishna Reddy
-- Purpose     : Rollback script to drop 'questionnaire' tables
-- Jira Task   : BEN-645
-- ============================================================================
DO $$
BEGIN
    DROP INDEX IF EXISTS task.idx_questionnaire_question_group_0;
    DROP TABLE IF EXISTS task.questionnaire_question_group CASCADE;
    DROP TABLE IF EXISTS task.questionnaire_question CASCADE;
    DROP TABLE IF EXISTS task.questionnaire CASCADE;
END
$$;