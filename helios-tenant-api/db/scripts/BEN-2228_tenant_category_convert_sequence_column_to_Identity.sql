-- =============================================================================
-- 🧩 Script    : Convert Sequence Column to IDENTITY
-- 📌 Purpose   : Converts an existing sequence-based column to PostgreSQL IDENTITY.
-- 👨‍💻 Author    : Srikanth Kodam
-- 📅 Date      : 2025-12-08
-- 🧾 Jira      : BEN-2228
-- ⚙️ Inputs    :
--      - Schema  : tenant
--      - Table   : category
--      - Column  : id
-- 📝 Notes     :
--      - Safe to re-run
--      - Existing data is NOT affected
-- =============================================================================

DO
$$
DECLARE
    v_schema_name  TEXT := 'tenant';
    v_table_name   TEXT := 'category';
    v_column_name  TEXT := 'id';
    v_is_identity  TEXT;
BEGIN
    -- Check if column is already IDENTITY
    SELECT is_identity
    INTO v_is_identity
    FROM information_schema.columns
    WHERE table_schema = v_schema_name
      AND table_name   = v_table_name
      AND column_name  = v_column_name;

    -- Convert to IDENTITY 
    IF v_is_identity <> 'YES' THEN

        -- Drop old default sequence
        EXECUTE format(
            'ALTER TABLE %I.%I ALTER COLUMN %I DROP DEFAULT;',
            v_schema_name, v_table_name, v_column_name
        );

        -- Add IDENTITY (no sequence name hardcoded)
        EXECUTE format(
            'ALTER TABLE %I.%I ALTER COLUMN %I ADD GENERATED ALWAYS AS IDENTITY;',
            v_schema_name, v_table_name, v_column_name
        );

        RAISE NOTICE '✅ Column %.%.% converted to IDENTITY', v_schema_name, v_table_name, v_column_name;

    ELSE
        RAISE NOTICE 'ℹ️ Column already IDENTITY — skipped.';
    END IF;
END
$$;
