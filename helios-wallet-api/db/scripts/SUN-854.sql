-- ============================================================================
-- 🚀 Script    : Update wallet type labels for OTC related
-- 📌 Purpose   : Update wallet type labels for OTC related to lower case
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
        ('HFC', 'Flex benefit', 'OTC, grocery, copay assist, home modifications, utilities, pay at the pump/rideshare and pest control'),
        ('HFO', 'Flex benefit', 'OTC, grocery, home modifications, utilities, pay at the pump/rideshare and pest control'),
        ('OGT', 'Flex benefit', 'OTC, grocery, dental, vision, hearing, and transportation'),
        ('DOT', 'Flex benefit', 'OTC, dental, vision, hearing, and transportation'),
        ('UGT', 'Flex benefit', 'OTC, dental, vision, hearing, and transportation'),
        ('OGP', 'Flex benefit', 'OTC, grocery, and copay assist'),
        ('OCP', 'Flex benefit', 'OTC & copay assist'),
        ('CFO', 'Flex benefit', 'OTC and grocery')
    ) AS t(short_label, wallet_type_name, wallet_type_label)
)
UPDATE wallet.wallet_type wt
SET wallet_type_label = m.wallet_type_label,
    update_user       = 'SYSTEM',
    update_ts         = NOW()
FROM mapping m
WHERE wt.short_label = m.short_label
  AND wt.delete_nbr = 0
  AND wt.is_external_sync = true;

COMMIT;