-- =====================================================
-- Onboarding Flow Configuration Tables
-- =====================================================

-- 1. COMPONENT TYPE TABLE
CREATE TABLE IF NOT EXISTS tenant.component_type (
    pk BIGINT GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) PRIMARY KEY,
    component_type VARCHAR(100) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    create_ts TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,
    update_ts TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    create_user VARCHAR(50) NOT NULL,
    update_user VARCHAR(50),
    delete_nbr BIGINT DEFAULT 0 NOT NULL
);

-- 2. COMPONENT CATALOGUE TABLE
CREATE TABLE IF NOT EXISTS tenant.component_catalogue (
    pk BIGINT GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) PRIMARY KEY,
    component_type_fk BIGINT NOT NULL REFERENCES tenant.component_type(pk),
    component_name VARCHAR(100) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    create_ts TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,
    update_ts TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    create_user VARCHAR(50) NOT NULL,
    update_user VARCHAR(50),
    delete_nbr BIGINT DEFAULT 0 NOT NULL
);


-- 3. FLOW TABLE
CREATE TABLE IF NOT EXISTS tenant.flow (
    pk BIGINT GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) PRIMARY KEY,
    tenant_code VARCHAR(50) NOT NULL,
    cohort_code VARCHAR(50),
    flow_name VARCHAR(100) NOT NULL,
    version_nbr INT NOT NULL,
    effective_start_ts TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    effective_end_ts TIMESTAMP WITHOUT TIME ZONE,
    create_ts TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,
    update_ts TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    create_user VARCHAR(50) NOT NULL,
    update_user VARCHAR(50),
    delete_nbr BIGINT DEFAULT 0 NOT NULL,
    CONSTRAINT uq_flow UNIQUE (tenant_code, cohort_code, version_nbr)
);

CREATE INDEX idx_flow_tenant_cohort ON tenant.flow(tenant_code, cohort_code);

-- 4. FLOW STEP TABLE
CREATE TABLE IF NOT EXISTS tenant.flow_step (
    pk BIGINT GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) PRIMARY KEY,
    flow_fk BIGINT NOT NULL REFERENCES tenant.flow(pk),
    step_idx INT NOT NULL,
    current_component_catalogue_fk BIGINT NOT NULL REFERENCES tenant.component_catalogue(pk),
    on_success_component_catalogue_fk BIGINT REFERENCES tenant.component_catalogue(pk),
    on_failure_component_catalogue_fk BIGINT REFERENCES tenant.component_catalogue(pk),
    create_ts TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,
    update_ts TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    create_user VARCHAR(50) NOT NULL,
    update_user VARCHAR(50),
    delete_nbr BIGINT DEFAULT 0 NOT NULL
);


-- 5. CONSUMER FLOW PROGRESS TABLE
CREATE TABLE IF NOT EXISTS huser.consumer_flow_progress (
    pk BIGINT GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) PRIMARY KEY,
    consumer_code VARCHAR(50) NOT NULL,
    tenant_code VARCHAR(50) NOT NULL,
    cohort_code VARCHAR(50),
    flow_fk BIGINT NOT NULL REFERENCES tenant.flow(pk),
    version_nbr INT NOT NULL,
    flow_step_pk BIGINT REFERENCES tenant.flow_step(pk),
    status VARCHAR(50),
    context_json JSONB DEFAULT '{}'::jsonb NOT NULL,
    create_ts TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,
    update_ts TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    create_user VARCHAR(50) NOT NULL,
    update_user VARCHAR(50),
    delete_nbr BIGINT DEFAULT 0 NOT NULL,
    CONSTRAINT uq_consumer_flow UNIQUE (consumer_code, tenant_code, cohort_code, delete_nbr)
);

CREATE INDEX idx_consumer_flow_progress_consumer ON huser.consumer_flow_progress(consumer_code);
CREATE INDEX idx_consumer_flow_progress_tenant_cohort ON huser.consumer_flow_progress(tenant_code, cohort_code);

-- 6. CONSUMER ONBOARDING PROGRESS HISTORY TABLE
CREATE TABLE IF NOT EXISTS huser.consumer_flow_progress_history (
    pk BIGINT GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) PRIMARY KEY,
    consumer_flow_progress_fk BIGINT NOT NULL REFERENCES huser.consumer_flow_progress(pk),
    consumer_code VARCHAR(50) NOT NULL,
    tenant_code VARCHAR(50) NOT NULL,
    cohort_code VARCHAR(50),
    flow_fk BIGINT NOT NULL REFERENCES tenant.flow(pk),
    version_nbr INT NOT NULL,
    from_flow_step_pk BIGINT,--Step transitioned from (NULL on start)
    to_flow_step_pk BIGINT,--Step transitioned to (NULL on complete/end)
    outcome VARCHAR(50) NOT NULL,
    create_ts TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,
    update_ts TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    create_user VARCHAR(50) NOT NULL,
    update_user VARCHAR(50),
    delete_nbr BIGINT DEFAULT 0 NOT NULL
);

CREATE INDEX idx_onboarding_progress_hist_consumer ON huser.consumer_flow_progress_history(consumer_code);
CREATE INDEX idx_onboarding_progress_hist_tenant_cohort ON huser.consumer_flow_progress_history(tenant_code, cohort_code);

-- =====================================================
-- GRANTS
-- =====================================================
GRANT ALL ON ALL TABLES IN SCHEMA tenant TO hadminusr;
GRANT ALL ON ALL TABLES IN SCHEMA huser TO hadminusr;

GRANT DELETE, INSERT, UPDATE, SELECT ON ALL TABLES IN SCHEMA tenant TO happusr;
GRANT DELETE, INSERT, UPDATE, SELECT ON ALL TABLES IN SCHEMA huser TO happusr;

GRANT SELECT ON ALL TABLES IN SCHEMA tenant TO hrousr;
GRANT SELECT ON ALL TABLES IN SCHEMA huser TO hrousr;
