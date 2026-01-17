-- ===============================================================================
-- Script   : Normalize all environment-specific URLs in tenant_attr
-- Author   : Vinod Ullaganti
-- Date     : 2025-07-22
-- Jira     : SOCT-1375
-- Purpose  : 🔁 Find and fix any '.uat.', '.qa.', or '.integ.' in URL strings
--             across any field inside tenant_attr
-- ===============================================================================
-- ⚠️ CRITICAL CAUTION:
--     🚨 This script updates **ALL URLs** within tenant_attr JSONB structure
--     🔍 Please **manually validate all existing URLs** in the target tenant_attr
--     🧪 Ensure correct format is being applied for:
--         ✅ QA → '.qa.'
--         ✅ UAT → '.uat.'
--         ✅ INTEG → '.integ.'
--         ✅ PROD → removes '.uat.', '.qa.', '.integ.' completely
--     ❗ DO NOT run this blindly on PROD without reviewing exact changes first
-- ===============================================================================

DO $$
DECLARE
	-- 🔁 Replace with actual "Navitus" tenant code
    v_tenant_code TEXT := '<NAVITUS-TENANT-CODE>';
	v_env TEXT := '<ENV>';  -- 🔁 Change to DEV, QA, UAT, INTEG, or PROD
    v_env_specific_url TEXT;
    v_json JSONB :=
	'{
	  "ux": {
		"themeColors": {
		  "accent1": "#0B3B60",
		  "accent2": "#E27025",
		  "accent3": "#FFFFFF",
		  "accent4": "#293A40",
		  "accent5": "#949C99",
		  "accent6": "#212121",
		  "accent7": "#63B6677F",
		  "accent8": "#151E23",
		  "headerBgColor": "#0D1C3D",
		  "primaryaccent": "#094B7A"
		},
		"walletColors": {
		  "textFgColor": "#212121",
		  "walletBgColor": "#FFFFFF",
		  "leftToEarnColor": "#133B71",
		  "buttonLabelColor": "#F2F2F2",
		  "redeemButtonColor": "#FFFFFF",
		  "strokeEarnedColor": "#E27025",
		  "strokeSegmentColor": "#D3D6DC",
		  "availableSpendBgColor": "#E27025"
		},
		"taskTileColors": {
		  "textColor": "#545454",
		  "contentBgColor": "#5F6062",
		  "activeTabBgColor": "#5F6062",
		  "tileLinear1Color": "#003B71",
		  "tileLinear2Color": "#326F91",
		  "tileLinear3Color": "#2E8807",
		  "tileLinear4Color": "#48D50B",
		  "inProgressBgColor": "#0B3B60",
		  "completedTileBgColor": "#868C92",
		  "inProgressTextFgColor": "#326F91",
		  "sectionHeaderTitleColor": "#5F6062"
		},
		"agreementsColors": {
		  "agreeButtonColor": "#0B3B60",
		  "agreeButtonLabelColor": "#FFFFFF",
		  "declineButtonLabelColor": "#0B3B60"
		},
		"onboardingColors": {
		  "stepActiveColor": "#E27025"
		},
		"entriesWalletColors": {
		  "headerBgColor": "#217AB5",
		  "contentBgColor": "#D3D6DC",
		  "headerTextColor": "#FFFFFF",
		  "contentTextColor": "#133B71"
		},
		"commonColors": {
		  "button1Color": "#E27025",
		  "button1TextColor": "#FFFFFF",
		  "paginationDotActiveColor":"#0D1C3D",
		  "paginationDotNonActiveColor":"#D3D6DC"
		}
	  },
	  "trivia": {
		"startupTrivia": true
	  },
	  "startPage": "BENEFITS-HOME",
	  "consumerWallet": {
		"ownerMaximum": 125,
		"walletMaximum": 125,
		"individualWallet": true,
		"contributorMaximum": 125,
		"splitRewardOverflow": true
	  },
	  "jitfTimeOffset": 40,
	  "isHybridRewards": true,
	  "nonMonetaryOnly": false,
	  "membershipWallet": {
		"earnMaximum": 125
	  },
	  "justInTimeFunding": true,
	  "benefitsCardArtUrl": "https://app-static.qa.sunnyrewards.com/public/images/Navitus+Card+2.png",
	  "walletSplitEnabled": false,
	  "adventureEarnMaximum": 100,
	  "nonMonetaryLearnMore": {
		"headerText": "How your entries work",
		"descriptionText": "Each month there will be 5 drawings to win a $200 gift card, spendable at a variety of popular online retailers. Any entries not applied in the current month will automatically roll over to the next month, giving you multiple chances to win!"
	  },
	  "spinwheelTaskEnabled": false,
	  "monetarySplashScreens": [
		"/assets/images/ftue-swiper1.svg"
	  ],
	  "onboardingFlowEnabled": true,
	  "costcoMembershipSupport": false,
	  "nonMonetaryDrawingDates": [
		"06/01/2025",
		"07/01/2025",
		"08/01/2025",
		"09/01/2025",
		"10/01/2025",
		"11/01/2025",
		"12/01/2025"
	  ],
	  "nonMonetaryFTUEFileName": "/assets/images/bf_nonmononly_ftue_new.svg",
	  "spendOnGiftCardDisabled": true,
	  "bf_monetarySplashScreens": [
		"/assets/images/bf_ftue-swiper1.svg",
		"/assets/images/bf_ftue-swiper2.svg"
	  ],
	  "disableMembershipDollars": false,
	  "nonMonetarySplashScreens": [
		"/assets/images/ftue-swiper2.svg"
	  ],
	  "nonmonetaryPrizesEnabled": true,
	  "autosweepSweepstakesReward": false,
	  "bf_nonMonetarySplashScreens": [
		"/assets/images/bf_ftue-swiper2.svg"
	  ],
	  "nonMonetaryOfficialRulesUrl": "https://sunnybenefits.com/official-rules-navitus-2025",
	  "pickAPurseOnboardingEnabled": false,
	  "maxAllowedPickAPurseSelection": 1,
	  "pickAPurseFundTransferEnabled": true,
	  "completionToggleAvailableActions": true,
	  "supportLiveTransferToRewardsPurse": false,
	  "supportLiveTransferWhileProcessingNonMonetary": false
	}'::jsonb;  -- 🔁 Paste your full tenant_attr JSON

BEGIN

	-- 🧠 Resolve environment-specific static URL
    CASE v_env
        WHEN 'DEV' THEN
            v_env_specific_url := 'https://app-static.dev.sunnyrewards.com';
        WHEN 'QA' THEN
            v_env_specific_url := 'https://app-static.qa.sunnyrewards.com';
        WHEN 'UAT' THEN
            v_env_specific_url := 'https://app-static.uat.sunnyrewards.com';
        WHEN 'INTEG' THEN
            v_env_specific_url := 'https://app-static.integ.sunnyrewards.com';
        WHEN 'PROD' THEN
            v_env_specific_url := 'https://app-static.sunnyrewards.com';
        ELSE
            RAISE EXCEPTION '❌ Invalid environment [%]. Please choose from DEV, QA, UAT, INTEG, PROD.', v_env;
    END CASE;

    -- 🔁 Replace env-specific URL in JSON structure
	v_json := jsonb_set(
		v_json,
		'{benefitsCardArtUrl}',
		to_jsonb(v_env_specific_url || '/public/images/Kaiser_Permanente_Card_2.png'),
		true
	);

    -- Update the tenant_attr
    UPDATE tenant.tenant
    SET tenant_attr = v_json
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0;

    RAISE NOTICE '✅ tenant_attr updated for %', v_tenant_code;
END $$;