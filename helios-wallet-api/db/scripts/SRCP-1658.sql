-- Drop the existing unique constraint on prev_wallet_txn_code column
ALTER TABLE wallet.transaction
DROP CONSTRAINT IF EXISTS transaction_prev_wallet_txn_code_key;