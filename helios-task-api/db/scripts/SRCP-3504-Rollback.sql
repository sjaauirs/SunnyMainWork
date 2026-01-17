-- Script to remove the wallet_transaction_code column if it exists

ALTER TABLE task.consumer_task DROP COLUMN IF EXISTS wallet_transaction_code;