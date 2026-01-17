CREATE UNIQUE INDEX idx_consumer_login_access_token
ON huser.consumer_login(access_token);

CREATE INDEX idx_consumer_login_1
ON huser.consumer_login(consumer_id, delete_nbr);

CREATE UNIQUE INDEX idx_consumer_tenant_code_mem_nbr_delete_nbr
ON huser.consumer(tenant_code, mem_nbr, delete_nbr);