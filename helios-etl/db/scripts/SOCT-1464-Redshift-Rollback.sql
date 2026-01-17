-- This is a rollback script for SOCT-1464-Redshift.sql
-- Revoke future privileges
ALTER DEFAULT PRIVILEGES IN SCHEMA etl_outbound
REVOKE SELECT, INSERT, UPDATE, DELETE ON TABLES FROM appuser;

-- Revoke privileges from existing tables
REVOKE SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA etl_outbound FROM appuser;

-- Revoke schema usage
REVOKE USAGE ON SCHEMA etl_outbound FROM appuser;

-- Drop tables (child first due to FK or dependencies)
DROP TABLE IF EXISTS etl_outbound.member_import_file_data;
DROP TABLE IF EXISTS etl_outbound.member_import_file;

-- Drop schema
DROP SCHEMA IF EXISTS etl_outbound;
