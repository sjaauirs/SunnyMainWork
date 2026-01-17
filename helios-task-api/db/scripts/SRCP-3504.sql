--Add wallet_transaction_code column to task.consumer_task tabke it column not exists
ALTER TABLE task.consumer_task ADD COLUMN IF NOT EXISTS wallet_transaction_code VARCHAR(50) NULL;