-- moves funds from consumer back to master wallet as an adjustment
do $$
  declare prevtxn varchar;
  declare last_txn_detail_id bigint;
  declare txn_code varchar;
  declare prev_bal numeric;
  declare now_ts timestamp without time zone;
  declare master_wallet_id bigint = 3;
  declare consumer_wallet_id bigint = 5570;
  declare consumer_code varchar = 'cmr-8d6d5a962bc94f70a35670d18b2d643b';
  declare adjust_amt numeric = 45.0;
begin

  now_ts = (select now() at time zone 'utc');

  INSERT INTO wallet.transaction_detail(
    transaction_detail_type, consumer_code, task_reward_code, notes, redemption_ref, redemption_item_description, create_ts, create_user, delete_nbr, reward_description)
    VALUES ('ADJUSTMENT', consumer_code, null, null, null, null, now_ts, 'SYSTEM', 0, null);
  last_txn_detail_id = (select max(transaction_detail_id) from wallet.transaction_detail);

  txn_code = (select 'txn-' || replace(cast(gen_random_uuid() as varchar), '-', ''));

  -- consumer wallet
  prevtxn = (select cast(wallet_id as varchar) || ':' || transaction_code from wallet.transaction where wallet_id=consumer_wallet_id and delete_nbr=0 order by transaction_id desc limit 1);
  prev_bal = (select balance from wallet.wallet where wallet_id=consumer_wallet_id and delete_nbr=0);
  INSERT INTO wallet.transaction(
    wallet_id, transaction_code, transaction_type, previous_balance, transaction_amount, balance, prev_wallet_txn_code, create_ts, create_user, delete_nbr, transaction_detail_id)
    VALUES (consumer_wallet_id, txn_code, 'S', prev_bal,
        adjust_amt, prev_bal - adjust_amt, prevtxn, now_ts, 'SYSTEM', 0, last_txn_detail_id);
  update wallet.wallet set balance=balance - adjust_amt, total_earned=total_earned - adjust_amt where wallet_id=consumer_wallet_id and delete_nbr=0;

  -- master wallet
  prevtxn = (select cast(wallet_id as varchar) || ':' || transaction_code from wallet.transaction where wallet_id=master_wallet_id and delete_nbr=0 order by transaction_id desc limit 1);
  prev_bal = (select balance from wallet.wallet where wallet_id=master_wallet_id and delete_nbr=0);
  INSERT INTO wallet.transaction(
    wallet_id, transaction_code, transaction_type, previous_balance, transaction_amount, balance, prev_wallet_txn_code, create_ts, create_user, delete_nbr, transaction_detail_id)
    VALUES (master_wallet_id, txn_code, 'A', prev_bal,
        adjust_amt, prev_bal + adjust_amt, prevtxn, now_ts, 'SYSTEM', 0, last_txn_detail_id);
  update wallet.wallet set balance=balance + adjust_amt where wallet_id=master_wallet_id and master_wallet=true and delete_nbr=0;

end $$;
