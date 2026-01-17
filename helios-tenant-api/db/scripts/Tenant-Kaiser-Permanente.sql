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
	-- 🔁 Replace with actual "Kaiser Permanente" tenant code
    v_tenant_code TEXT := '<KP-TENANT-CODE>';
	v_env TEXT := '<ENV>';  -- 🔁 Change to DEV, QA, UAT, INTEG, or PROD
    v_env_specific_url TEXT;
    v_json JSONB :=
	'{
	  "ux": {
		"themeColors": {
		  "accent1": "#0078B3",
		  "accent2": "#0078B3",
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
		  "redeemButtonColor": "#FFFFFF",
		  "strokeEarnedColor": "#57A635",
		  "strokeSegmentColor": "#D3D6DC",
		  "availableSpendBgColor": "#217AB5"
		},
		"taskTileColors": {
		  "textColor": "#545454",
		  "contentBgColor": "#5F6062",
		  "activeTabBgColor": "#0078B3",
		  "tileLinear1Color": "#003B71",
		  "tileLinear2Color": "#44B8F3",
		  "tileLinear3Color": "#2E8807",
		  "tileLinear4Color": "#48D50B",
		  "inProgressBgColor": "#0B3B60",
		  "completedTileBgColor": "#868C92",
		  "inProgressTextFgColor": "#0078B3",
		  "sectionHeaderTitleColor": "#0078B3"
		},
		"agreementColors": {
		  "agreeButtonColor": "#0078B3",
		  "agreeButtonLabelColor": "#FFFFFF",
		  "declineButtonLabelColor": "#0078B3"
		},
		"onboardingColors": {
		  "stepActiveColor": "#0078B3"
		},
		"entriesWalletColors": {
		  "headerBgColor": "#217AB5",
		  "contentBgColor": "#D3D6DC",
		  "headerTextColor": "#FFFFFF",
		  "contentTextColor": "#133B71"
		},
		"commonColors": {
		  "button1Color": "#0078b3",
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
		"ownerMaximum": 150,
		"walletMaximum": 150,
		"individualWallet": true,
		"contributorMaximum": 150,
		"splitRewardOverflow": true
	  },
	  "dataDeleteLink": "https://healthy.kaiserpermanente.org/privacy",
	  "jitfTimeOffset": 40,
	  "kpRedirectLink": "https://hreg1.kaiserpermanente.org/colorado/secure/inner-door",
	  "isHybridRewards": true,
	  "liveChatbotInfo": {
		"orgId": "00DD7000000jTxL",
		"siteURL": "https://sunnybenefits1--connect.sandbox.my.site.com/ESWCindyEnhanced1750155574700",
		"scrt2URL": "https://sunnybenefits1--connect.sandbox.my.salesforce-scrt.com",
		"serviceName": "Cindy_Enhanced",
		"enableLiveChatbot": true,
		"bootstrapScriptURL": "https://sunnybenefits1--connect.sandbox.my.site.com/ESWCindyEnhanced1750155574700/assets/js/bootstrap.min.js"
	  },
	  "nonMonetaryOnly": false,
	  "hideRedeemHeader": true,
	  "kpGaRedirectLink": "https://hreg2.kaiserpermanente.org/georgia/secure/inner-door",
	  "membershipWallet": {
		"earnMaximum": 150
	  },
	  "adventuresEnabled": true,
	  "isRedirectSignout": true,
	  "justInTimeFunding": true,
	  "privacyPolicyLink": "https://healthy.kaiserpermanente.org/privacy",
	  "benefitsCardArtUrl": "https://app-static.uat.sunnyrewards.com/public/images/Kaiser_Permanente_Card_2.png",
	  "privacyPolicyLinks": [
		{
		  "url": "https://healthy.kaiserpermanente.org/privacy ",
		  "languageCode": "en-US"
		},
		{
		  "url": "https://espanol.kaiserpermanente.org/es/privacy ",
		  "languageCode": "es"
		}
	  ],
	  "walletSplitEnabled": false,
	  "declineRedirectLink": "https://healthy.kaiserpermanente.org/front-door",
	  "adventureEarnMaximum": 100,
	  "nonMonetaryLearnMore": {
		"headerText": "How your entries work",
		"descriptionText": "Each month there will be 20 drawings to win a $150 gift card, spendable at a variety of popular online retailers. Any entries not applied in the current month will automatically roll over to the next month, giving you multiple chances to win!"
	  },
	  "spinwheelTaskEnabled": false,
	  "disableOnboardingFlow": true,
	  "monetarySplashScreens": [
		"/assets/images/ftue-swiper1.svg"
	  ],
	  "costcoMembershipSupport": false,
	  "languageSpecificContent": {
		"es": {
		  "dataDeleteLink": "https://espanol.kaiserpermanente.org/es/privacy",
		  "privacyPolicyLink": "https://espanol.kaiserpermanente.org/es/privacy",
		  "declineRedirectLink": "https://espanol.kaiserpermanente.org/es/front-door"
		},
		"en-US": {
		  "dataDeleteLink": "https://espanol.kaiserpermanente.org/es/front-door",
		  "privacyPolicyLink": "https://healthy.kaiserpermanente.org/privacy",
		  "declineRedirectLink": "https://healthy.kaiserpermanente.org/front-door"
		}
	  },
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
	  "supportLiveTransferToRewardsPurse": true,
	  "supportLiveTransferWhileProcessingNonMonetary": false,
	  "includeHeaderFooter": true,
	  "websiteTitle": "Complete Healthy Activities to Earn Rewards | Kaiser Permanente",
	  "hideNameInitials": true
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