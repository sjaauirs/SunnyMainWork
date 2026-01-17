
-- ============================================================================
-- 🔁 Script: Rollback DOB-Based Cohort Assignment Script Changes
-- 📌 Purpose: Rolls back changes made by the cohort assignment script based on 
--             person's DOB (even/odd logic) and task_reward_code linkage.
-- 🧑 Author  : Rakesh Pernati
-- 📅 Date    : 2025-07-29
-- 🧾 Jira    : SOCT-1492
-- ⚠️ Inputs : Tenant Code
-- 📤 Action : Soft-deletes tenant_task_reward_script entry, removes script 
--             linkage, updates script JSON (if used elsewhere), and soft-deletes 
--             the script if not linked elsewhere.
-- ============================================================================
-- 🔔 NOTE:
-- 📥 Tenant Code will be passed as an input

DO $$
DECLARE
    v_input_tenant_code TEXT := '<KP-TENANT-CODE>';  -- 🔁 Input tenant code
    v_script_code TEXT := 'src-20809ec035f24caf936f65e8e354975b';
    v_task_external_code TEXT := 'play_heal_triv';

    v_script_id INT;
    v_task_reward_code TEXT;
    v_existing_script_source TEXT;
    v_updated_script_source TEXT;
    v_existing_json TEXT;
    v_existing_jsonb JSONB;
    v_filtered_jsonb JSONB;
    v_tenant_task_reward_script_id INT;
BEGIN
    -- 🎯 Fetch task_reward_code
    SELECT task_reward_code INTO v_task_reward_code
    FROM task.task_reward
    WHERE tenant_code = v_input_tenant_code
      AND delete_nbr = 0
      AND task_external_code = v_task_external_code;

    IF v_task_reward_code IS NULL THEN
        RAISE NOTICE '⚠️ No task_reward_code found for external code: %', v_task_external_code;
        RETURN;
    END IF;

    -- 📜 Fetch script info
    SELECT script_id, script_source INTO v_script_id, v_existing_script_source
    FROM admin.script
    WHERE script_code = v_script_code
      AND delete_nbr = 0;

    IF v_script_id IS NULL THEN
        RAISE NOTICE '⚠️ No script found for script_code: %', v_script_code;
        RETURN;
    END IF;

    -- 🔍 Get tenant_task_reward_script_id for delete_nbr
    SELECT tenant_task_reward_script_id INTO v_tenant_task_reward_script_id
    FROM admin.tenant_task_reward_script
    WHERE script_id = v_script_id
      AND tenant_code = v_input_tenant_code
      AND task_reward_code = v_task_reward_code
      AND delete_nbr = 0;

    -- 🧹 Soft delete tenant_task_reward_script link
    UPDATE admin.tenant_task_reward_script
    SET delete_nbr = v_tenant_task_reward_script_id,
        update_ts = CURRENT_TIMESTAMP,
        update_user = 'Kumar'
    WHERE script_id = v_script_id
      AND tenant_code = v_input_tenant_code
      AND task_reward_code = v_task_reward_code
      AND delete_nbr = 0;

    RAISE NOTICE '🗑️ Soft-deleted tenant_task_reward_script entry for task_reward_code: %', v_task_reward_code;

    -- 🔄 Check for remaining active links
    IF NOT EXISTS (
        SELECT 1 FROM admin.tenant_task_reward_script
        WHERE script_id = v_script_id
          AND delete_nbr = 0
    ) THEN
        -- ❌ No active links — soft delete script itself
        UPDATE admin.script
        SET delete_nbr = v_script_id,
            update_ts = CURRENT_TIMESTAMP,
            update_user = 'Kumar'
        WHERE script_id = v_script_id;

        RAISE NOTICE '🗑️ Soft-deleted script as it has no more active links.';
    ELSE
        -- 🧱 Extract JSON from script_source
        v_existing_json := regexp_replace(v_existing_script_source, '.*const taskRewardCohorts = ', '', 'g');
        v_existing_json := regexp_replace(v_existing_json, ';.*', '', 'g');
        v_existing_jsonb := v_existing_json::jsonb;

        -- 🔍 Filter out the entry to rollback
        v_filtered_jsonb := (
            SELECT jsonb_agg(elem)
            FROM jsonb_array_elements(v_existing_jsonb) AS elem
            WHERE elem->>'task_reward_code' <> v_task_reward_code
        );

        -- 🔁 Replace JSON in script source
        v_updated_script_source := replace(
            v_existing_script_source,
            v_existing_jsonb::text,
            COALESCE(v_filtered_jsonb, '[]'::jsonb)::text
        );

        -- ✍️ Update script with new JSON
        UPDATE admin.script
        SET script_source = v_updated_script_source,
            update_ts = CURRENT_TIMESTAMP,
            update_user = 'Rollback'
        WHERE script_id = v_script_id;

        RAISE NOTICE '✅ Updated script_source by removing task_reward_code: %', v_task_reward_code;
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE '❌ Rollback Error: %', SQLERRM;
END $$;
