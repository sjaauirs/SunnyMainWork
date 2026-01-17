BEGIN;

ALTER TABLE wallet.wallet_type
ALTER COLUMN wallet_type_label TYPE varchar(80);

COMMIT;

BEGIN;

-- Define rollback mapping (short_label, old_name, old_label)
WITH mapping AS (
    SELECT *
    FROM (VALUES
        ('HFC', 'Comprehensive Living Support', 'Comprehensive Living Support'),
        ('HFO', 'Daily Living Support', 'Daily Living Support'),
        ('OGT', 'DOT with Grocery (OTC, Grocery, Dental, Vision, Hearing, and Transportation)', 'DOT with Grocery'),
        ('DOT', 'DOT (OTC, Dental, Vision, Hearing, and Transportation)', 'DOT'),
        ('UGT', 'UGT (OTC, Dental, Vision, Hearing, and Transportation)', 'DOT'),
        ('OGP', 'OTC, Grocery and Copay Assist', 'OTC, Grocery and Copay Assist'),
        ('OCP', 'OTC and Copay Assist', 'OTC and Copay Assist'),
        ('CFO', 'OTC and Grocery', 'OTC and Grocery'),
        ('OTC', 'Over the Counter', 'OTC')
    ) AS t(short_label, wallet_type_name, wallet_type_label)
)
UPDATE wallet.wallet_type wt
SET wallet_type_name  = m.wallet_type_name,
    wallet_type_label = m.wallet_type_label,
    update_user       = 'SYSTEM',
    update_ts         = NOW()
FROM mapping m
WHERE wt.short_label = m.short_label
  AND wt.delete_nbr = 0
  AND wt.is_external_sync = true;

COMMIT;
