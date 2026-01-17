-- Update existing suspense wallet to map OTC
UPDATE wallet.wallet_type SET wallet_type_name= 'OTC Suspense', wallet_type_label = 'OTC', short_label = 'OTC'
WHERE wallet_type_code = 'wat-bc8f4f7c028d479f900f0af794e385c8'

-- Insert new wallet type for Food suspense wallet
INSERT INTO wallet.wallet_type (wallet_type_code, wallet_type_name, create_ts, update_ts, create_user, update_user, delete_nbr, wallet_type_label, short_label, is_external_sync)
SELECT 'wat-01b4a568c1814449b64168bb434127b7', 'Food Suspense', '2024-06-25 16:31:22.723817', '-infinity', 'per-915325069cdb42c783dd4601e1d27704', NULL, '0', 'FOOD', 'FOOD', False
WHERE NOT EXISTS (SELECT 1 FROM wallet.wallet_type WHERE wallet_type_code = 'wat-01b4a568c1814449b64168bb434127b7');

-- Insert new wallet type for Vision suspense wallet
INSERT INTO wallet.wallet_type (wallet_type_code, wallet_type_name, create_ts, update_ts, create_user, update_user, delete_nbr, wallet_type_label, short_label, is_external_sync)
SELECT 'wat-ae61bfba4c5c4494b25dbd91cba20c27', 'Vision Suspense', '2024-06-25 16:32:22.723817', '-infinity', 'per-915325069cdb42c783dd4601e1d27704', NULL, '0', 'VISION', 'VISION', False
WHERE NOT EXISTS (SELECT 1 FROM wallet.wallet_type WHERE wallet_type_code = 'wat-ae61bfba4c5c4494b25dbd91cba20c27');

-- Insert new wallet type for Hearing suspense wallet
INSERT INTO wallet.wallet_type (wallet_type_code, wallet_type_name, create_ts, update_ts, create_user, update_user, delete_nbr, wallet_type_label, short_label, is_external_sync)
SELECT 'wat-1dfebf5b3dc04aa5b09b256191a1888c', 'Hearing Suspense', '2024-06-25 16:33:22.723817', '-infinity', 'per-915325069cdb42c783dd4601e1d27704', NULL, '0', 'HEARING', 'HEARING', False
WHERE NOT EXISTS (SELECT 1 FROM wallet.wallet_type WHERE wallet_type_code = 'wat-1dfebf5b3dc04aa5b09b256191a1888c');

-- Insert new wallet type for Utility wallet
INSERT INTO wallet.wallet_type (wallet_type_code, wallet_type_name, create_ts, update_ts, create_user, update_user, delete_nbr, wallet_type_label, short_label, is_external_sync)
SELECT 'wat-5ec72c2b2867407b913e28283777ebdf', 'Utility', '2024-06-25 16:34:22.723817', '-infinity', 'per-915325069cdb42c783dd4601e1d27704', NULL, '0', 'Utility', 'Utility', False
WHERE NOT EXISTS (SELECT 1 FROM wallet.wallet_type WHERE wallet_type_code = 'wat-5ec72c2b2867407b913e28283777ebdf');

-- Insert new wallet type for Utility wallet with is_external_sync true
INSERT INTO wallet.wallet_type (wallet_type_code, wallet_type_name, create_ts, update_ts, create_user, update_user, delete_nbr, wallet_type_label, short_label, is_external_sync)
SELECT 'wat-cb5eff3b86114410ad3fdce38d2b4001', 'Utility', '2024-06-25 16:34:22.723817', '-infinity', 'per-915325069cdb42c783dd4601e1d27704', NULL, '0', 'Utility', 'Utility', True
WHERE NOT EXISTS (SELECT 1 FROM wallet.wallet_type WHERE wallet_type_code = 'wat-cb5eff3b86114410ad3fdce38d2b4001');

-- Insert new wallet type for Utility suspense wallet
INSERT INTO wallet.wallet_type (wallet_type_code, wallet_type_name, create_ts, update_ts, create_user, update_user, delete_nbr, wallet_type_label, short_label, is_external_sync)
SELECT 'wat-22c1f94f535341f5ace684aed2a24cab', 'Utility Suspense', '2024-06-25 16:35:22.723817', '-infinity', 'per-915325069cdb42c783dd4601e1d27704', NULL, '0', 'UTILITY', 'UTILITY', False
WHERE NOT EXISTS (SELECT 1 FROM wallet.wallet_type WHERE wallet_type_code = 'wat-22c1f94f535341f5ace684aed2a24cab');