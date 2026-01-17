-- =================================================================================================================================
-- 🚀 Script    : Script for safe insertion/updation of "Your Voice Matters" task reward code in consumer
-- 📌 Purpose   : Introduce the "Your Voice Matters" task for KP tenants only, ensuring safe updates.
-- 🧑 Author    : Siva Krishna
-- 📅 Date      : 2025-10-07
-- 🧾 Jira      : https://sunnyrewards.atlassian.net/browse/RES-52
-- ⚠️ Inputs    : KP-TENANT-CODE
-- 📤 Output    : Safely updated or logged existing consumers
-- 🔗 Script URL: NA
-- 📝 Notes     : Script needs to be executed in sequence. This is only for KP tenant.
-- 🔢 Sequence Number: 3
-- ===================================================================================================================================

DO $$
DECLARE
    -- <Input Parameters>                                       
    v_tenant_code TEXT := '<KP-TENANT-CODE>'; -- KP tenant only

    -- <Variable Declarations>
    v_task_external_code TEXT := 'your_voic_matt';
    v_task_reward_code   TEXT;
    v_count              INT := 0;
    v_consumer RECORD;
    v_updated_count      INT := 0;
BEGIN
    RAISE NOTICE '[Info] Starting consumer update for tenant_code=% (task_external_code=%)', v_tenant_code, v_task_external_code;

    -- Step 1: Get task reward code
    SELECT task_reward_code
    INTO v_task_reward_code
    FROM task.task_reward
    WHERE task_external_code = v_task_external_code
      AND tenant_code = v_tenant_code
      AND delete_nbr = 0;

    -- If not found, raise error and stop
    IF v_task_reward_code IS NULL THEN
        RAISE NOTICE '[Error] Task reward code not found for task_external_code=%', v_task_external_code;
        RETURN;
    END IF;

    RAISE NOTICE '[Info] Found task_reward_code=% for tenant_code=%', v_task_reward_code, v_tenant_code;

    -- Step 2: Process each consumer safely
    FOR v_consumer IN
        SELECT consumer_code, consumer_attr
        FROM huser.consumer
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0
    LOOP
        -- Skip if consumer already has this reward code
        IF v_consumer.consumer_attr->'surveyTaskRewardCodes' @> jsonb_build_array(jsonb_build_object(v_task_reward_code, v_count)) THEN
            RAISE NOTICE '[Info] ConsumerCode= "%" already contains task_reward_code=% with count=% — skipping update.',
                         v_consumer.consumer_code, v_task_reward_code, v_count;
        ELSE
            -- Otherwise update safely
            UPDATE huser.consumer
            SET consumer_attr = jsonb_set(
                                    COALESCE(consumer_attr, '{}'::jsonb),
                                    '{surveyTaskRewardCodes}',
                                    (
                                        CASE
                                            WHEN consumer_attr->'surveyTaskRewardCodes' IS NOT NULL
                                            THEN (consumer_attr->'surveyTaskRewardCodes')::jsonb
                                                 || jsonb_build_array(jsonb_build_object(v_task_reward_code, v_count))
                                            ELSE jsonb_build_array(jsonb_build_object(v_task_reward_code, v_count))
                                        END
                                    ),
                                    true
                                ),
                update_ts = NOW(),
                update_user = 'SYSTEM'
            WHERE consumer_code = v_consumer.consumer_code
              AND tenant_code = v_tenant_code
              AND delete_nbr = 0;

            v_updated_count := v_updated_count + 1;

            RAISE NOTICE '[Success] Updated consumer_code=% with new task_reward_code=% (count=%)',
                         v_consumer.consumer_code, v_task_reward_code, v_count;
        END IF;
    END LOOP;

    -- Step 3: Summary log
    IF v_updated_count > 0 THEN
        RAISE NOTICE '[Summary] % consumer record(s) updated successfully for tenant_code=%',
                     v_updated_count, v_tenant_code;
    ELSE
        RAISE NOTICE '[Summary] No consumers required an update — all already had task_reward_code=% for tenant_code=%',
                     v_task_reward_code, v_tenant_code;
    END IF;

    RAISE NOTICE '[Info] Consumer update process completed for tenant_code=%', v_tenant_code;
END $$;
