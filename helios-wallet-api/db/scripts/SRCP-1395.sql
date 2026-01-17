ALTER TABLE wallet.wallet_type 
ADD COLUMN IF NOT EXISTS wallet_type_label varchar(80) NOT NULL DEFAULT 'test';

UPDATE wallet.wallet_type SET wallet_type_label='Healthy Actions' WHERE wallet_type_code='wat-2d62dcaf2aa4424b9ff6c2ddb5895077';
