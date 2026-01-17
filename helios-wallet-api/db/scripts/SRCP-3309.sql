-- Insert new record into wallet.wallet_type table if it does not exist
INSERT INTO wallet.wallet_type (wallet_type_code, wallet_type_name, create_ts, update_ts, create_user, update_user, delete_nbr, wallet_type_label, short_label, is_external_sync)
SELECT 'wat-c616be8b26c1449687ad0afda1512e7c', 'Health Actions Membership Reward', CURRENT_TIMESTAMP, null, 'per-915325069cdb42c783dd4601e1d27704', null, 0, 'Health Actions Membership Reward', 'Membership', false
WHERE NOT EXISTS (
    SELECT 1 FROM wallet.wallet_type WHERE wallet_type_code = 'wat-c616be8b26c1449687ad0afda1512e7c'
);