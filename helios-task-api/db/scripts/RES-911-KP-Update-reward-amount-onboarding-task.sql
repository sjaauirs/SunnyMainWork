-- ========================================================================================================
-- 🚀 Script    : Update Onboarding Survey Reward Amount
-- 📌 Purpose   : For given tenant_code, set rewardAmount = 0 when IsOnBoardingSurvey = true in task_reward_config_json
-- 🧑 Author    : Siva Krishna
-- 📅 Date      : 29-OCT-2025
-- 🧾 Jira      : RES-1002
-- ⚙️ Inputs    : KP-TENANT-CODE
-- 📤 Output    : Updates reward.rewardAmount to 0 for onboarding survey tasks
-- 🔗 Script URL: NA
-- 📝 Notes     : Only modifies rewardAmount; other keys remain intact
-- ========================================================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>';  -- 🔹 Input tenant code
    rec RECORD;
BEGIN
    RAISE NOTICE '[Information] Starting update for tenant_code=%', v_tenant_code;

    FOR rec IN
        SELECT task_reward_id, reward
        FROM task.task_reward
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0
          AND (task_reward_config_json ->> 'IsOnBoardingSurvey')::BOOLEAN = TRUE
    LOOP
        UPDATE task.task_reward
        SET reward = jsonb_set(reward, '{rewardAmount}', '0'::jsonb, true),
            update_ts = NOW(),
            update_user = 'SYSTEM'
        WHERE task_reward_id = rec.task_reward_id;

        RAISE NOTICE '[Updated] task_reward_id=% | rewardAmount set to 0', rec.task_reward_id;
    END LOOP;

    RAISE NOTICE '[Information] Script execution completed successfully.';
END $$;
