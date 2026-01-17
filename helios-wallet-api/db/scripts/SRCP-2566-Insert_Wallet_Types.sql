INSERT INTO wallet.wallet_type (wallet_type_code, wallet_type_name, create_ts, update_ts, create_user, update_user, delete_nbr, wallet_type_label, short_label, is_external_sync)
SELECT 'wat-fa7232d4d778400fb2960a30f81d66f8', 'Vision', '2024-03-14 20:12:09.664', NULL, 'per-915325069cdb42c783dd4601e1d27704', NULL, '0', 'Vision', 'Vision', True
WHERE NOT EXISTS (SELECT 1 FROM wallet.wallet_type WHERE wallet_type_code = 'wat-fa7232d4d778400fb2960a30f81d66f8');

UPDATE wallet.wallet_type SET is_external_sync = true WHERE wallet_type_code = 'wat-fa7232d4d778400fb2960a30f81d66f8' ;

INSERT INTO wallet.wallet_type (wallet_type_code, wallet_type_name, create_ts, update_ts, create_user, update_user, delete_nbr, wallet_type_label, short_label, is_external_sync)
SELECT 'wat-28c467ebc2154735934d4fd6afac1fbb', 'Hearing', '2024-03-14 20:12:09.664', NULL, 'per-915325069cdb42c783dd4601e1d27704', NULL, '0', 'Hearing', 'Hearing', True
WHERE NOT EXISTS (SELECT 1 FROM wallet.wallet_type WHERE wallet_type_code = 'wat-28c467ebc2154735934d4fd6afac1fbb');

UPDATE wallet.wallet_type SET is_external_sync = true WHERE wallet_type_code = 'wat-28c467ebc2154735934d4fd6afac1fbb' ;

INSERT INTO wallet.wallet_type (wallet_type_code, wallet_type_name, create_ts, update_ts, create_user, update_user, delete_nbr, wallet_type_label, short_label, is_external_sync)
SELECT 'wat-35b3ac62d74b4119a5f630c9b6446035', 'Food', '2024-03-14 20:12:09.664', NULL, 'per-915325069cdb42c783dd4601e1d27704', NULL, '0', 'Food', 'Food', True
WHERE NOT EXISTS (SELECT 1 FROM wallet.wallet_type WHERE wallet_type_code = 'wat-35b3ac62d74b4119a5f630c9b6446035');

UPDATE wallet.wallet_type SET is_external_sync = true WHERE wallet_type_code = 'wat-35b3ac62d74b4119a5f630c9b6446035' ;

INSERT INTO wallet.wallet_type (wallet_type_code, wallet_type_name, create_ts, update_ts, create_user, update_user, delete_nbr, wallet_type_label, short_label, is_external_sync)
SELECT 'wat-e2c6076b59db46febd8d76fd019ae0b0', 'Health Actions Entries Redemption', '2024-03-14 20:12:09.66442', NULL, 'per-915325069cdb42c783dd4601e1d27704', NULL, '0', 'Entries Redemption', 'Entries Redemption', False
WHERE NOT EXISTS (SELECT 1 FROM wallet.wallet_type WHERE wallet_type_code = 'wat-e2c6076b59db46febd8d76fd019ae0b0');

UPDATE wallet.wallet_type SET is_external_sync = false WHERE wallet_type_code = 'wat-2d62dcaf2aa4424b9ff6c2ddb5895077' ;

INSERT INTO wallet.wallet_type (wallet_type_code, wallet_type_name, create_ts, update_ts, create_user, update_user, delete_nbr, wallet_type_label, short_label, is_external_sync)
SELECT 'wat-55f05107b41642c39e7a3b459223cbd8', 'Health Actions Reward', '2023-06-13 15:46:50.210476', '-infinity', 'per-915325069cdb42c783dd4601e1d27704', NULL, '0', 'Rewards', 'Rewards', True
WHERE NOT EXISTS (SELECT 1 FROM wallet.wallet_type WHERE wallet_type_code = 'wat-55f05107b41642c39e7a3b459223cbd8');

UPDATE wallet.wallet_type SET is_external_sync = true WHERE wallet_type_code = 'wat-55f05107b41642c39e7a3b459223cbd8' ;

INSERT INTO wallet.wallet_type (wallet_type_code, wallet_type_name, create_ts, update_ts, create_user, update_user, delete_nbr, wallet_type_label, short_label, is_external_sync)
SELECT 'wat-52f1d48ac6b64b01ab4a5ecb48557319', 'Funding', '2023-06-04 15:50:01.065699', NULL, 'per-915325069cdb42c783dd4601e1d27704', NULL, '0', 'test', 'test', False
WHERE NOT EXISTS (SELECT 1 FROM wallet.wallet_type WHERE wallet_type_code = 'wat-52f1d48ac6b64b01ab4a5ecb48557319');

INSERT INTO wallet.wallet_type (wallet_type_code, wallet_type_name, create_ts, update_ts, create_user, update_user, delete_nbr, wallet_type_label, short_label, is_external_sync)
SELECT 'wat-274bd71345804f09928cf451dc0f6239', 'Health Actions Redemption', '2023-06-04 15:50:01.065699', '-infinity', 'per-915325069cdb42c783dd4601e1d27704', NULL, '0', 'test', 'test', False
WHERE NOT EXISTS (SELECT 1 FROM wallet.wallet_type WHERE wallet_type_code = 'wat-274bd71345804f09928cf451dc0f6239');

INSERT INTO wallet.wallet_type (wallet_type_code, wallet_type_name, create_ts, update_ts, create_user, update_user, delete_nbr, wallet_type_label, short_label, is_external_sync)
SELECT 'wat-4b364ed612f04034bf732b355d84f368', 'Over the Counter', '2024-01-19 03:09:30.315267', '-infinity', 'per-915325069cdb42c783dd4601e1d27704', NULL, '0', 'Over the Counter (OTC)', 'OTC', True
WHERE NOT EXISTS (SELECT 1 FROM wallet.wallet_type WHERE wallet_type_code = 'wat-4b364ed612f04034bf732b355d84f368');

UPDATE wallet.wallet_type SET is_external_sync = true WHERE wallet_type_code = 'wat-4b364ed612f04034bf732b355d84f368' ;

INSERT INTO wallet.wallet_type (wallet_type_code, wallet_type_name, create_ts, update_ts, create_user, update_user, delete_nbr, wallet_type_label, short_label, is_external_sync)
SELECT 'wat-cb7234r5d778599fb2960a30f82f67r9', 'Vision', '2024-03-14 20:12:09.664', NULL, 'per-915325069cdb42c783dd4601e1d27704', NULL, '0', 'Vision', 'Vision', False
WHERE NOT EXISTS (SELECT 1 FROM wallet.wallet_type WHERE wallet_type_code = 'wat-cb7234r5d778599fb2960a30f82f67r9');

INSERT INTO wallet.wallet_type (wallet_type_code, wallet_type_name, create_ts, update_ts, create_user, update_user, delete_nbr, wallet_type_label, short_label, is_external_sync)
SELECT 'wat-29d568uiy2154735934d6ty8afac2edf', 'Hearing', '2024-03-14 20:12:09.664', NULL, 'per-915325069cdb42c783dd4601e1d27704', NULL, '0', 'Hearing', 'Hearing', False
WHERE NOT EXISTS (SELECT 1 FROM wallet.wallet_type WHERE wallet_type_code = 'wat-29d568uiy2154735934d6ty8afac2edf');

INSERT INTO wallet.wallet_type (wallet_type_code, wallet_type_name, create_ts, update_ts, create_user, update_user, delete_nbr, wallet_type_label, short_label, is_external_sync)
SELECT 'wat-35d4rt62d77y6119a6t730c9b6447457', 'Food', '2024-03-14 20:12:09.664', NULL, 'per-915325069cdb42c783dd4601e1d27704', NULL, '0', 'Food', 'Food', False
WHERE NOT EXISTS (SELECT 1 FROM wallet.wallet_type WHERE wallet_type_code = 'wat-35d4rt62d77y6119a6t730c9b6447457');

INSERT INTO wallet.wallet_type (wallet_type_code, wallet_type_name, create_ts, update_ts, create_user, update_user, delete_nbr, wallet_type_label, short_label, is_external_sync)
SELECT 'wat-4b364fg722f04034cv732b355d84f479', 'Over the Counter', '2024-01-19 03:09:30.315', '-infinity', 'per-915325069cdb42c783dd4601e1d27704', NULL, '0', 'OTC', 'OTC', False
WHERE NOT EXISTS (SELECT 1 FROM wallet.wallet_type WHERE wallet_type_code = 'wat-4b364fg722f04034cv732b355d84f479');

INSERT INTO wallet.wallet_type (wallet_type_code, wallet_type_name, create_ts, update_ts, create_user, update_user, delete_nbr, wallet_type_label, short_label, is_external_sync)
SELECT 'wat-bc8f4f7c028d479f900f0af794e385c8', 'SUSPENSE_WALLET', '2024-05-01 16:39:22.723817', '-infinity', 'per-915325069cdb42c783dd4601e1d27704', NULL, '0', 'SUSPENSE', 'SUSPENSE', False
WHERE NOT EXISTS (SELECT 1 FROM wallet.wallet_type WHERE wallet_type_code = 'wat-bc8f4f7c028d479f900f0af794e385c8');

INSERT INTO wallet.wallet_type (wallet_type_code, wallet_type_name, create_ts, update_ts, create_user, update_user, delete_nbr, wallet_type_label, short_label, is_external_sync)
SELECT 'wat-c3b091232e974f98aeceb495d2a9f916', 'Health Actions Sweepstakes Entries', '2024-01-31 15:50:01.065699', '-infinity', 'per-915325069cdb42c783dd4601e1d27704', NULL, '0', 'Sweepstakes Entries', 'Sweepstakes Entries', False
WHERE NOT EXISTS (SELECT 1 FROM wallet.wallet_type WHERE wallet_type_code = 'wat-c3b091232e974f98aeceb495d2a9f916');
