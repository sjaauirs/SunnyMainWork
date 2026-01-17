-- =============================================================================
-- 🧩 Script    : Sync IDENTITY / Sequence Values with Table Data
-- 📂 Schema    : tenant
-- 📌 Purpose   :
--   - Identifies IDENTITY/serial columns in the target tables.
--   - Finds the underlying sequence for each column.
--   - Updates the sequence value to match the current MAX value in the table.
--   - Prevents duplicate key violations when inserting new records.
-- 👨‍💻 Author  : Srikanth Kodam
-- 📅 Date      : 2025-12-08
-- 🧾 Jira      : BEN-2228
-- ⚙️ Inputs    :
--   - Target Schema : tenant
--   - Target Tables : category, wallet_category
-- 📤 Output    :
--   - Sequences updated to align with existing data.
--   - Logs displayed via RAISE NOTICE for tracking.
-- 📝 Notes     :
--   - Safe to re-run (idempotent).
--   - Does not modify existing table data.
--   - Only affects sequence/identity generators.
-- =============================================================================

DO $$
DECLARE
    rec RECORD;
    _max BIGINT;
    _seq TEXT;
BEGIN
    FOR rec IN
        SELECT table_schema, table_name, column_name
        FROM information_schema.columns
        WHERE identity_generation IS NOT NULL
          AND table_schema = 'tenant'
          AND table_name IN ('category', 'wallet_category')
        ORDER BY table_schema, table_name
    LOOP
        RAISE NOTICE '🔍 Checking table %.%, column %', rec.table_schema, rec.table_name, rec.column_name;

        -- Get the sequence name dynamically
        EXECUTE format(
            'SELECT pg_get_serial_sequence(%L, %L)',
            rec.table_schema || '.' || rec.table_name,
            rec.column_name
        ) INTO _seq;

        IF _seq IS NULL THEN
            RAISE NOTICE '❌ No sequence found for %.% column %',
                rec.table_schema, rec.table_name, rec.column_name;
            CONTINUE;
        END IF;

        -- Get the current MAX value from the column
        EXECUTE format(
            'SELECT MAX(%I) FROM %I.%I',
            rec.column_name, rec.table_schema, rec.table_name
        ) INTO _max;

        IF _max IS NULL THEN
            RAISE NOTICE '⚠️ Table %.% has no rows. Sequence % not updated.',
                rec.table_schema, rec.table_name, _seq;
            CONTINUE;
        END IF;

        -- Update the sequence to the MAX value
        EXECUTE format(
            'SELECT setval(%L, %s, true)',
            _seq, _max
        );

        RAISE NOTICE '✅ Sequence % set to % for %.% column %',
            _seq, _max, rec.table_schema, rec.table_name, rec.column_name;
    END LOOP;
END $$;
