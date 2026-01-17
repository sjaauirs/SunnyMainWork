ALTER TABLE task.task_detail 
ADD COLUMN IF NOT EXISTS tenant_code varchar(50) not null default 'ten-ecada21e57154928a2bb959e8365b8b4';

DROP INDEX task.task_id_language_code_delete_nbr;

CREATE UNIQUE INDEX IF NOT EXISTS task.idx_task_detail_1
    ON task.task_detail (task_id, language_code, tenant_code, delete_nbr)
    TABLESPACE pg_default;

INSERT INTO task.task_detail(
	task_id, language_code, task_header, task_description, terms_of_service_id, create_ts, update_ts, create_user, update_user, delete_nbr, task_cta_button_text, tenant_code)
(SELECT task_id, language_code, task_header, task_description, terms_of_service_id, create_ts, update_ts, create_user, update_user, delete_nbr, task_cta_button_text, 'ten-8d9e6f00eec8436a8251d55ff74b1642'
	FROM task.task_detail where tenant_code='ten-ecada21e57154928a2bb959e8365b8b4' order by task_id asc);

INSERT INTO task.task_detail(
	task_id, language_code, task_header, task_description, terms_of_service_id, create_ts, update_ts, create_user, update_user, delete_nbr, task_cta_button_text, tenant_code)
(SELECT task_id, language_code, task_header, task_description, terms_of_service_id, create_ts, update_ts, create_user, update_user, delete_nbr, task_cta_button_text, 'ten-87cfa20e9d7140ec9294ae3342d79db0'
	FROM task.task_detail where tenant_code='ten-ecada21e57154928a2bb959e8365b8b4' order by task_id asc);

