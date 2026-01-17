-- =====================================================
-- Rollback Script - Drop Onboarding Flow Configuration Tables
-- =====================================================

-- Drop in reverse dependency order

-- 6. CONSUMER ONBOARDING PROGRESS HISTORY TABLE
DROP TABLE IF EXISTS huser.consumer_onboarding_progress_history CASCADE;

-- 5. CONSUMER FLOW PROGRESS TABLE
DROP TABLE IF EXISTS huser.consumer_flow_progress CASCADE;

-- 4. FLOW STEP TABLE
DROP TABLE IF EXISTS tenant.flow_step CASCADE;

-- 3. FLOW TABLE
DROP TABLE IF EXISTS tenant.flow CASCADE;

-- 2. COMPONENT CATALOGUE TABLE
DROP TABLE IF EXISTS tenant.component_catalogue CASCADE;

-- 1. COMPONENT TYPE TABLE
DROP TABLE IF EXISTS tenant.component_type CASCADE;

