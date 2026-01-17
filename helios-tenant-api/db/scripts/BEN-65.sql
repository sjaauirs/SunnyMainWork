
/*
================================================================================
Script Purpose:
--------------
This script updates the `tenant_attr` column in the `tenant.tenant` 
table for a specific tenant. 
Update tenant_attr for HAP teannt as HAP requirement
================================================================================
*/
update tenant.tenant set tenant_attr = '{
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
    "commonColors": {
      "textColor": "#181D27",
      "textColor2": "#45474D",
      "buttonColor": "#181D27",
      "button1Color": "#0078b3",
      "primaryColor": "#FF7200",
      "screenBgColor": "#FFFFFF",
      "contentBgColor": "#FBF8F6",
      "secondaryColor": "#66615C",
      "buttonTextColor": "#FFFFFF",
      "contentBgColor2": "#F3EFE9",
      "button1TextColor": "#FFFFFF",
      "buttonTextColor2": "#181D27",
      "paginationDotActiveColor": "#0D1C3D",
      "paginationDotNonActiveColor": "#D3D6DC"
    },
    "footerColors": {
      "footerBgColor": "#66615C",
      "footerTextColor": "#FFFFFF"
    },
    "headerColors": {
      "headerBgColor": "#FBF8F6",
      "headerTextColor": "#FFFFFF",
      "headerTopBorderColor": "#FF7200",
      "headerBottomBorderColor": "#CCC8C2"
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
    }
  },
  "trivia": {
    "startupTrivia": true
  },
  "startPage": "BENEFITS-HOME",
  "websiteTitle": "Complete Healthy Activities to Earn Rewards | HAP",
  "consumerWallet": {
    "ownerMaximum": 150,
    "walletMaximum": 150,
    "individualWallet": true,
    "contributorMaximum": 150,
    "splitRewardOverflow": true
  },
  "dataDeleteLink": "https://www.hap.org/privacy",
  "jitfTimeOffset": 40,
  "kpRedirectLink": "",
  "headerImageUrls": {
    "headerMobileIconUrl": "https://app-static.dev.sunnyrewards.com/public/images/hap_icon.png",
    "headerDesktopIconUrl": "https://app-static.dev.sunnyrewards.com/public/images/hap_icon.png"
  },
  "isHybridRewards": true,
  "liveChatbotInfo": {
    "orgId": "00DD7000000jTxL",
    "siteURL": "https://sunnybenefits1--connect.sandbox.my.site.com/ESWCindyEnhanced1750155574700",
    "scrt2URL": "https://sunnybenefits1--connect.sandbox.my.salesforce-scrt.com",
    "serviceName": "Cindy_Enhanced",
    "enableLiveChatbot": false,
    "bootstrapScriptURL": "https://sunnybenefits1--connect.sandbox.my.site.com/ESWCindyEnhanced1750155574700/assets/js/bootstrap.min.js"
  },
  "nonMonetaryOnly": false,
  "hideNameInitials": true,
  "hideRedeemHeader": true,
  "kpGaRedirectLink": "",
  "membershipWallet": {
    "earnMaximum": 150
  },
  "TriviaMobileImage": "https://app-static.dev.sunnyrewards.com/public/images/common_trivia.png",
  "adventuresEnabled": false,
  "isRedirectSignout": true,
  "justInTimeFunding": true,
  "privacyPolicyLink": "https://www.hap.org/privacy",
  "TriviaDesktopImage": "https://app-static.dev.sunnyrewards.com/public/images/common_trivia.png",
  "benefitsCardArtUrl": "https://app-static.dev.sunnyrewards.com/public/images/Kaiser_Permanente_Card_2.png",
  "privacyPolicyLinks": [
    {
      "url": "https://www.hap.org/privacy",
      "languageCode": "en-US"
    }
  ],
  "walletSplitEnabled": false,
  "declineRedirectLink": "https://www.hap.org/",
  "includeHeaderFooter": true,
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
  "includeLanguageDropdown": false,
  "languageSpecificContent": {
    "es": {
      "dataDeleteLink": "https://www.hap.org/",
      "privacyPolicyLink": "https://www.hap.org/privacy",
      "declineRedirectLink": "https://www.hap.org/"
    },
    "en-US": {
      "dataDeleteLink": "https://www.hap.org/",
      "privacyPolicyLink": "https://www.hap.org/privacy",
      "declineRedirectLink": "https://www.hap.org/"
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
  "supportLiveTransferWhileProcessingNonMonetary": false
}'
where tenant_code = '<HAP-TenantCode>' and delete_nbr = 0; -- Replace <HAP TenantCode>, ex: ten-b4e920d3f6f74496ab533d1a9a8ef9e4