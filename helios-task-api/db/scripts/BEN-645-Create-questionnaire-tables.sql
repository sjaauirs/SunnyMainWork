-- ============================================================================
--  Script: Create Questionnaire Tables
--  Purpose: Replica of trivia tables with updated names these tables to store trivia, survey and feedback actions
--  Author : Siva Krishna Reddy
-- Jira Task: BEN-645
-- ============================================================================
DO $$
BEGIN
    -- Create task.questionnaire
    CREATE TABLE IF NOT EXISTS task.questionnaire
    (
        questionnaire_id bigint GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) PRIMARY KEY,
        questionnaire_code character varying(50) NOT NULL,
        task_reward_id bigint NOT NULL,
        cta_task_external_code character varying(80) COLLATE pg_catalog."default",
        config_json jsonb NOT NULL,
        create_ts timestamp without time zone NOT NULL,
        update_ts timestamp without time zone,
        create_user character varying(50) COLLATE pg_catalog."default" NOT NULL,
        update_user character varying(50) COLLATE pg_catalog."default",
        delete_nbr bigint NOT NULL,
        CONSTRAINT fk_questionnaire_task_reward FOREIGN KEY (task_reward_id)
            REFERENCES task.task_reward (task_reward_id) MATCH SIMPLE
            ON UPDATE NO ACTION
            ON DELETE NO ACTION
    );

    -- Create task.questionnaire_question
    CREATE TABLE IF NOT EXISTS task.questionnaire_question
    (
        questionnaire_question_id bigint GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) PRIMARY KEY,
        questionnaire_question_code character varying(50) NOT NULL,
        questionnaire_json jsonb NOT NULL,
        create_ts timestamp without time zone NOT NULL,
        update_ts timestamp without time zone,
        create_user character varying(50) COLLATE pg_catalog."default" NOT NULL,
        update_user character varying(50) COLLATE pg_catalog."default",
        delete_nbr bigint NOT NULL,
        question_external_code character varying(80) COLLATE pg_catalog."default" NOT NULL
    );

    -- Create task.questionnaire_question_group
    CREATE TABLE IF NOT EXISTS task.questionnaire_question_group
    (
        questionnaire_question_group_id bigint GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) PRIMARY KEY,
        questionnaire_id bigint NOT NULL,
        questionnaire_question_id bigint NOT NULL,
        sequence_nbr integer NOT NULL,
        create_ts timestamp without time zone NOT NULL,
        update_ts timestamp without time zone,
        create_user character varying(50) COLLATE pg_catalog."default" NOT NULL,
        update_user character varying(50) COLLATE pg_catalog."default",
        delete_nbr bigint NOT NULL,
        valid_start_ts timestamp without time zone DEFAULT ((now() AT TIME ZONE 'UTC'::text) - '1 mon'::interval),
        valid_end_ts timestamp without time zone DEFAULT ((now() AT TIME ZONE 'UTC'::text) + '1 mon'::interval),
        CONSTRAINT fk_questionnaire_id FOREIGN KEY (questionnaire_id)
            REFERENCES task.questionnaire (questionnaire_id) MATCH SIMPLE
            ON UPDATE NO ACTION
            ON DELETE NO ACTION,
        CONSTRAINT fk_questionnaire_question_id FOREIGN KEY (questionnaire_question_id)
            REFERENCES task.questionnaire_question (questionnaire_question_id) MATCH SIMPLE
            ON UPDATE NO ACTION
            ON DELETE NO ACTION
    );

END
$$;

-- ============================================================================
--  Unique Index for task.questionnaire_question_group
-- ============================================================================
-- Index: idx_questionnaire_question_group_0

-- DROP INDEX IF EXISTS task.idx_questionnaire_question_group_0;

CREATE UNIQUE INDEX IF NOT EXISTS idx_questionnaire_question_group_0
    ON task.questionnaire_question_group USING btree
    (questionnaire_id ASC NULLS LAST,
     questionnaire_question_id ASC NULLS LAST,
     delete_nbr ASC NULLS LAST)
    TABLESPACE pg_default;

-- ============================================================================
-- Grant Permissions
-- ============================================================================
GRANT ALL ON TABLE task.questionnaire TO hadminusr;
GRANT DELETE, INSERT, UPDATE, SELECT ON TABLE task.questionnaire TO happusr;
GRANT SELECT ON TABLE task.questionnaire TO hrousr;

GRANT ALL ON TABLE task.questionnaire_question TO hadminusr;
GRANT DELETE, INSERT, UPDATE, SELECT ON TABLE task.questionnaire_question TO happusr;
GRANT SELECT ON TABLE task.questionnaire_question TO hrousr;

GRANT ALL ON TABLE task.questionnaire_question_group TO hadminusr;
GRANT DELETE, INSERT, UPDATE, SELECT ON TABLE task.questionnaire_question_group TO happusr;
GRANT SELECT ON TABLE task.questionnaire_question_group TO hrousr;


