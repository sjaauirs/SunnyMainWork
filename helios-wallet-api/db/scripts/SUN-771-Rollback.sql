-- ============================================================================
-- Script    : Update wallet_type table for HLTHLVNG short_label
-- Purpose   : Updates wallet_type_name and wallet_type_label to 'Healthy living' 
--             for records where short_label = 'HLTHLVNG', delete_nbr = 0, 
--             and is_external_sync = TRUE.
-- Author    : Preeti
-- Date      : 2025-10-28
-- Jira      : SUN-771
-- ============================================================================

DO $$
BEGIN
    UPDATE wallet.wallet_type
    SET 
        wallet_type_name  = 'Healthy living',
        wallet_type_label = 'Healthy living',
        update_ts         = NOW(),
        update_user       = 'SYSTEM'
    WHERE short_label      = 'HLTHLVNG'
      AND delete_nbr       = 0
      AND is_external_sync = TRUE;

    RAISE NOTICE 'wallet_type updated successfully for short_label = HLTHLVNG';
END $$;
