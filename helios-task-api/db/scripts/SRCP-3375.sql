DO $$ 
BEGIN
  -- Check if the column's current data type is not jsonb
  IF NOT EXISTS (
    SELECT 1
    FROM information_schema.columns
    WHERE table_schema='task' and table_name = 'task_reward'
      AND column_name = 'reward'
      AND data_type = 'jsonb'
  ) THEN
  
    -- Alter the column's data type to jsonb if it's not already
   ALTER TABLE task.task_reward
  ALTER COLUMN reward
  SET DATA TYPE jsonb USING reward::jsonb;
 END IF;
 --Update script
 UPDATE task.task_reward
SET reward = jsonb_set(
                jsonb_set(
                    -- Convert the reward (which is a character varying) to jsonb
                    task.task_reward.reward,  -- Cast the string to jsonb
                    '{rewardType}', 
                    CASE 
                        WHEN reward_type.reward_type_name = 'MONETARY_DOLLARS' THEN '"MONETARY_DOLLARS"'
                        WHEN reward_type.reward_type_name = 'SWEEPSTAKES_ENTRIES' THEN '"SWEEPSTAKES_ENTRIES"'
                        WHEN reward_type.reward_type_name = 'MEMBERSHIP_DOLLARS' THEN '"MEMBERSHIP_DOLLARS"'
                        ELSE '"UNKNOWN"' -- Default case in case of an unexpected value
                    END::jsonb
                ),
                '{membershipType}', 
                CASE 
                    WHEN reward_type.reward_type_name = 'MEMBERSHIP_DOLLARS' THEN '"COSTCO"'  -- Set COSTCO if membership type is MEMBERSHIP_DOLLARS
                    ELSE 'null'::jsonb  -- This will ensure that 'null' is properly set as a jsonb null
                END
            )
FROM task.reward_type
WHERE task.task_reward.reward_type_id = reward_type.reward_type_id
AND task.task_reward.reward IS NOT NULL;
END $$;

