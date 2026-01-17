-- =====================================================================
-- ♻️ Rollback Script: Soft delete admin.script entry
-- 📌 Purpose: Reverse script insert by marking delete_nbr = script_id
-- =====================================================================

DO $$
DECLARE
    v_script_code TEXT := 'src-5e0ebae1a21c49b5af3591991e8e3842';
    v_script_id BIGINT;
BEGIN
    -- Fetch script_id
    SELECT script_id INTO v_script_id
    FROM admin.script
    WHERE script_code = v_script_code
      AND delete_nbr = 0
    LIMIT 1;

    IF v_script_id IS NULL THEN
        RAISE NOTICE '⚠️ No matching script found to rollback for script_code: %', v_script_code;
        RETURN;
    END IF;

    -- Perform soft delete
    UPDATE admin.script
    SET delete_nbr = script_id,
        update_ts = CURRENT_TIMESTAMP,
        update_user = 'ROLLBACK'
    WHERE script_id = v_script_id
      AND delete_nbr = 0;

    RAISE NOTICE '♻️ Rolled back successfully. Soft deleted script_code: %, script_id: %',
        v_script_code, v_script_id;

EXCEPTION WHEN OTHERS THEN
    RAISE NOTICE '❌ Rollback Error: %', SQLERRM;
END $$;
