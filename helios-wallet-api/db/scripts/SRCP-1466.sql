ALTER TABLE wallet.wallet_type
ADD COLUMN IF NOT EXISTS short_label varchar(30) NOT NULL DEFAULT 'test'
