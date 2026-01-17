-- Insert new record into task.reward_type table if it does not exist
INSERT INTO task.reward_type (reward_type_name, reward_type_description, create_ts, update_ts, create_user, update_user, delete_nbr, reward_type_code)
SELECT 'MEMBERSHIP_DOLLARS', 'Money for memberships', CURRENT_TIMESTAMP, null, 'per-915325069cdb42c783dd4601e1d27704', null, 0, 'rtc-88f6fb24b04841a7b93b597e6fe36d91'
WHERE NOT EXISTS (
    SELECT 1 FROM task.reward_type WHERE reward_type_code = 'rtc-88f6fb24b04841a7b93b597e6fe36d91'
);