-- ============================================================================
-- 🚀 Script    : Revert wallet type labels for OTC related
-- 📌 Purpose   : Revert  wallet type labels for OTC related to lower case
-- 🧑 Author    : Neel Kunchakurti
-- 📅 Date      : 2025-10-31
-- 🧾 Jira      : SUN-854
-- ⚠️ Inputs    :  
-- 📤 Output    : 
-- 📝 Notes     : Existing keys remain intact
-- ============================================================================

BEGIN;

WITH mapping AS (
    SELECT *
    FROM (VALUES
        ('HFC', 'Flex benefit', 'OTC, Grocery, Copay Assist, Home Modifications, Utilities, Pay at the Pump/Rideshare and Pest Control'),
        ('HFO', 'Flex benefit', 'OTC, Grocery, Home Modifications, Utilities, Pay at the Pump/Rideshare and Pest Control'),
        ('OGT', 'Flex benefit', 'OTC, Grocery, Dental, Vision, Hearing, and Transportation'),
        ('DOT', 'Flex benefit', 'OTC, Dental, Vision, Hearing, and Transportation'),
        ('UGT', 'Flex benefit', 'OTC, Dental, Vision, Hearing, and Transportation'),
        ('OGP', 'Flex benefit', 'OTC, Grocery, and Copay Assist'),
        ('OCP', 'Flex benefit', 'OTC & Copay Assist'),
        ('CFO', 'Flex benefit', 'OTC and Grocery'),
        ('OTC', 'Flex benefit', 'Over-the-Counter')
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