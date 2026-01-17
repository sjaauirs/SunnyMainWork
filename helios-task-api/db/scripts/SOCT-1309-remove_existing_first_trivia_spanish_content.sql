-- Note: Replace <KP-TENANT-CODE> with actual KP Tenant Code before execution

DO $$
DECLARE
    rec RECORD;
    v_tenant_code TEXT := '<KP-TENANT-CODE>';        -- 🔹 Input tenant code
    v_task_external_code TEXT := 'play_heal_triv';                    
BEGIN
    RAISE NOTICE '🔍 Previewing trivia_questions where ''es'' key will be removed...';

    FOR rec IN
        SELECT tq.trivia_question_id
        FROM task.trivia_question tq
        JOIN task.trivia_question_group tqg ON tq.trivia_question_id = tqg.trivia_question_id
        JOIN task.trivia t ON tqg.trivia_id = t.trivia_id
        JOIN task.task_reward tr ON t.task_reward_id = tr.task_reward_id
        WHERE tr.task_external_code = v_task_external_code
          AND tr.tenant_code = v_tenant_code
          AND tr.delete_nbr = 0
          AND t.delete_nbr = 0
          AND tqg.delete_nbr = 0
          AND tq.delete_nbr = 0
          AND tq.trivia_json ? 'es'
    LOOP
        BEGIN
            -- 🔧 Perform the update
            UPDATE task.trivia_question
            SET trivia_json = trivia_json - 'es'
            WHERE trivia_question_id = rec.trivia_question_id;

            -- ✅ Log success
            RAISE NOTICE '✅ Removed ''es'' from trivia_question_id: %', rec.trivia_question_id;

        EXCEPTION WHEN OTHERS THEN
            -- ❌ Log failure
            RAISE NOTICE '❌ Failed to update trivia_question_id: % | Error: %', rec.trivia_question_id, SQLERRM;
        END;
    END LOOP;

    RAISE NOTICE '🎉 Update process completed.';
END $$;
