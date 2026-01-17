-- ============================================================================
-- 🚀 Script    : Alter Agreement File Name Column to JSONB
-- 📌 Purpose   : Converts the `agreement_file_name` column in `huser.consumer`
--                table from text to JSONB format if not already JSON/JSONB.
-- 🧑 Author    : KAWALPREET KAUR
-- 📅 Date      : 2025-10-06
-- 🧾 Jira      : RES-569
-- ⚠️ Inputs    : None
-- 📤 Output    : Alters column data type to JSONB
-- 🔗 Script URL: <Documentation or Confluence link, if any>
-- 📝 Notes     : Ensure the script is run with appropriate privileges.
-- ============================================================================

DO $$
BEGIN
  RAISE NOTICE '--- Starting: Altering huser.consumer.agreement_file_name to JSONB ---';

  -- Check if the column exists and is not already of type JSON/JSONB
  IF EXISTS (
      SELECT 1
      FROM information_schema.columns
      WHERE table_schema = 'huser'
        AND table_name = 'consumer'
        AND column_name = 'agreement_file_name'
        AND data_type NOT IN ('json', 'jsonb')
  ) THEN
    -- Alter column type to JSONB with proper conversion logic
    ALTER TABLE huser.consumer
    ALTER COLUMN agreement_file_name TYPE JSONB
    USING 
    CASE
      WHEN agreement_file_name IS NULL OR TRIM(agreement_file_name) = '' THEN NULL
      ELSE to_jsonb(agreement_file_name)
    END;

    RAISE NOTICE '✅ Column type successfully converted to JSONB.';

  ELSE
    RAISE NOTICE 'ℹ️ Column already JSONB or does not exist. No action taken.';
  END IF;

  RAISE NOTICE '--- Script completed successfully ---';
END $$;

--
--
--

-- ============================================================================
-- 🚀 Script    : Update Agreement File Name JSON Values
-- 📌 Purpose   : Updates `agreement_file_name` JSON values for consumers under
--                a specific tenant by mapping tenant name to agreement key.
-- 🧑 Author    : KAWALPREET KAUR
-- 📅 Date      : 2025-10-06
-- 🧾 Jira      : RES-569
-- ⚠️ Inputs    : v_tenant_code (TEXT)
-- 📤 Output    : Updates `agreement_file_name` JSONB values in huser.consumer.
-- 🔗 Script URL: <Documentation or Confluence link, if any>
-- 📝 Notes     : Ensure column type is already converted to JSONB before running.
-- ============================================================================

DO $$
BEGIN
  RAISE NOTICE '--- Starting update for all tenants ---';

  UPDATE huser.consumer AS c
  SET agreement_file_name = jsonb_build_object(
      'TermsAndConditions',
      c.agreement_file_name::text
  )
  WHERE c.agreement_file_name IS NOT NULL 
    AND c.agreement_file_name::text != '';

  RAISE NOTICE '✅ Update completed successfully for all tenants.';
END $$;

-- ============================================================================
-- 🚀 Script    : Alter Agreement File Name Column to JSONB
-- 📌 Purpose   : Converts the `agreement_file_name` column in `huser.consumer_history`
--                table from text to JSONB format if not already JSON/JSONB.
-- 🧑 Author    : KAWALPREET KAUR
-- 📅 Date      : 2025-10-06
-- 🧾 Jira      : RES-569
-- ⚠️ Inputs    : None
-- 📤 Output    : Alters column data type to JSONB
-- 🔗 Script URL: <Documentation or Confluence link, if any>
-- 📝 Notes     : Ensure the script is run with appropriate privileges.
-- ============================================================================

DO $$
BEGIN
  RAISE NOTICE '--- Starting: Altering huser.consumer_history.agreement_file_name to JSONB ---';

  -- Check if the column exists and is not already of type JSON/JSONB
  IF EXISTS (
      SELECT 1
      FROM information_schema.columns
      WHERE table_schema = 'huser'
        AND table_name = 'consumer_history'
        AND column_name = 'agreement_file_name'
        AND data_type NOT IN ('json', 'jsonb')
  ) THEN
    -- Alter column type to JSONB with proper conversion logic
    ALTER TABLE huser.consumer_history
    ALTER COLUMN agreement_file_name TYPE JSONB
    USING 
    CASE
      WHEN agreement_file_name IS NULL OR TRIM(agreement_file_name) = '' THEN NULL
      ELSE to_jsonb(agreement_file_name)
    END;

    RAISE NOTICE '✅ Column type successfully converted to JSONB.';

  ELSE
    RAISE NOTICE 'ℹ️ Column already JSONB or does not exist. No action taken.';
  END IF;

  RAISE NOTICE '--- Script completed successfully ---';
END $$;

--
--
--

-- ============================================================================
-- 🚀 Script    : Update Agreement File Name JSON Values
-- 📌 Purpose   : Updates `agreement_file_name` JSON values for consumer_history under
--                a specific tenant by mapping tenant name to agreement key.
-- 🧑 Author    : KAWALPREET KAUR
-- 📅 Date      : 2025-10-06
-- 🧾 Jira      : RES-569
-- ⚠️ Inputs    : v_tenant_code (TEXT)
-- 📤 Output    : Updates `agreement_file_name` JSONB values in huser.consumer.
-- 🔗 Script URL: <Documentation or Confluence link, if any>
-- 📝 Notes     : Ensure column type is already converted to JSONB before running.
-- ============================================================================

DO $$
BEGIN
  RAISE NOTICE '--- Starting update for all tenants ---';

  UPDATE huser.consumer_history AS c
  SET agreement_file_name = jsonb_build_object(
      'TermsAndConditions',
      c.agreement_file_name::text
  )
  WHERE c.agreement_file_name IS NOT NULL 
    AND c.agreement_file_name::text != '';

  RAISE NOTICE '✅ Update completed successfully for all tenants.';
END $$;


