-- ============================================================================
-- 🚀 Script    : Sweepstakes Wallet Type Updater
-- 📌 Purpose   : Adds/updates "sweepstakesWinnerRewardWalletTypeCode" inside 
--                tenant.tenant.tenant_option_json for given tenant codes.
-- 🧑 Author    : Siva Krishna Reddy
-- 📅 Date      : 2025-12-09
-- 🧾 Jira      : RES-1147
-- ⚠️ Inputs    : 
--                - v_tenant_codes (TEXT[]) : List of tenant codes to enable this feature
--                - v_wallet_type_code (TEXT) : Replace with Target WalletTypeCode(Ex: OTC, FOD, Healthy living)
-- 📤 Output    : Updates tenant_option_json idempotently
-- 🔗 Script URL: NA
-- 📝 Notes     : For consumers with multiple purses, the target purse is determined by the tenant setting SweepstakesWinnerRewardWalletTypeCode.
--                 This value drives which purse receives the sweepstakes reward and how the Deposit Instruction file is generated and processed.
-- ============================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY['<TENANT1>', '<TENANT2>']; -- Replace the list below with the tenant codes for which you want to enable the Direct Deposit mechanism.
    v_wallet_type_code TEXT := 'wat-2d62dcaf2aa4424b9ff6c2ddb5895077'; -- Replace with Target WalletTypeCode(Ex: OTC, FOD, Healthy living)

    v_row RECORD;
BEGIN
    RAISE NOTICE '🚀 Starting Sweepstakes Wallet Type JSON Update Script...';
    RAISE NOTICE '👉 Wallet Type Code to apply: %', v_wallet_type_code;
    RAISE NOTICE '👉 Tenants to update: %', v_tenant_codes;

    FOR v_row IN 
        SELECT tenant_code
        FROM tenant.tenant
        WHERE tenant_code = ANY(v_tenant_codes)
          AND delete_nbr = 0
    LOOP
        RAISE NOTICE '🔄 Processing tenant: %', v_row.tenant_code;

        UPDATE tenant.tenant t
        SET tenant_option_json = jsonb_set(
                                    COALESCE(t.tenant_option_json, '{}'::jsonb),
                                    '{sweepstakesWinnerRewardWalletTypeCode}',
                                    ('"' || v_wallet_type_code || '"')::jsonb,
                                    true
                                 )
        WHERE t.tenant_code = v_row.tenant_code;

        RAISE NOTICE '✅ Updated tenant: % (JSON value set or refreshed)', v_row.tenant_code;
    END LOOP;

    RAISE NOTICE '🎉 Completed script execution for all tenants.';
END $$;
