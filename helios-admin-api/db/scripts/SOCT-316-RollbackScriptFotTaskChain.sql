
-- ============================================================================
-- ⏪ Rollback Script: Delete from admin.script by script_code
-- 📌 Purpose : Removes the inserted script in case of rollback
-- 🧑 Author  : Vinod Ullaganti
-- 📅 Date    : 2025-05-23
-- 🧾 Jira    : SOCT-538, SOCT-754
-- ⚠️  Input  : script_code
-- ============================================================================
 
DO $$
DECLARE
    v_script_code VARCHAR := 'src-5e0ebae1a21c49b5af3591991e873857';
    v_deleted_ct  INTEGER;
BEGIN

    UPDATE admin.script
	SET delete_nbr = script_id
    WHERE script_code = v_script_code
      AND delete_nbr = 0
    RETURNING * INTO v_deleted_ct; 
    IF FOUND THEN
        RAISE NOTICE '✅ Rollback successful. Script with script_code "%" deleted.', v_script_code;
    ELSE
        RAISE NOTICE '⚠️ No script found with script_code "%". Nothing to rollback.', v_script_code;
    END IF; 
EXCEPTION WHEN OTHERS THEN
    RAISE NOTICE '❌ Rollback failed: %', SQLERRM;
END $$;


DO $$
DECLARE
    v_tenant_code TEXT := 'ten-a468348402cd438ea9a1005ae2faedb6';
    v_task_reward_codes_list TEXT := 'trw-f8f535c7dbd44962909bbef1666924d1';
    v_script_code TEXT := 'src-5e0ebae1a21c49b5af3591991e873857';
    v_script_type TEXT := 'TASK_COMPLETE_POST';

    v_script_id BIGINT;
    v_task_reward_code TEXT;