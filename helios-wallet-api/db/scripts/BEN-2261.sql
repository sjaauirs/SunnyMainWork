-- ============================================================================
-- 🚀 Script   : add_index_and_redeem_end_ts_to_wallet.sql
-- 📌 Purpose  : Add missing columns "index" and "redeem_end_ts" to wallet.wallet
-- 👨‍💻 Author  : Saurabh Jaiswal
-- 📅 Date      : 2025-12-02
-- ============================================================================

ALTER TABLE wallet.wallet
    ADD COLUMN IF NOT EXISTS "index" INTEGER DEFAULT 0;

ALTER TABLE wallet.wallet
    ADD COLUMN IF NOT EXISTS "redeem_end_ts" TIMESTAMP NULL;
	
	
	

