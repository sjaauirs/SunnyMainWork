-- =============================================================================
-- Rollback Script: Remove inserted wallet types for OTC, Grocery, Copay Assist,
-- DOT, Daily Living Support, and Comprehensive Living Support
-- Purpose   : Deletes wallet_type rows inserted by the forward migration
-- Jira    : BEN-60
-- =============================================================================

DELETE FROM wallet.wallet_type
WHERE wallet_type_code IN (
    -- OTC
    'wat-4b364fg722f04034cv732b355d84f479',
    'wat-4b364ed612f04034bf732b355d84f368',
    'wat-bc8f4f7c028d479f900f0af794e385c8',

    -- OTC and Grocery
    'wat-3509a5788e5246b18221582031cd10a3',
    'wat-bb06d4c12ac84213bc59bc2093421264',
    'wat-7502f7583a414a53b5bba944a58aeec9',

    -- OTC and Copay Assist
    'wat-14cfd51de64c46e4b927a7e8984474ea',
    'wat-7be788fb5115443eb0ead237b6c46cc4',
    'wat-1f1b825b43dd4d42a560606247ab26f8',

    -- OTC, Grocery and Copay Assist
    'wat-2422da2eb57b4a2c9acb24e4d593fba7',
    'wat-aca6aa177739432980e094b86567db7d',
    'wat-20bbb6af4d194fd5954ccfe955ee5bfb',

    -- DOT
    'wat-7ab9caa63bb14a6093649fbf3b97b0b4',
    'wat-5b0d5378af774c0381b67ed3e77d2fdd',
    'wat-4d931cf47a46485d94fd7d80051a7749',

    -- DOT with Grocery
    'wat-2ea762719bac47349aac36e7b2ade583',
    'wat-c583162a9130457289a09e28daaedc2e',
    'wat-6b61d3cab56c4e98b56da9f157e3133b',
	
	--UGT
	'wat-49812db3d9814dbca8eae2eba91722af',
	'wat-e207db6a8a0a460fbe852ce9c3fcbd54',
	'wat-3274e7cf318f4ba3a61228112d60229f',

    -- Daily Living Support
    'wat-44b999834ec344c88c1f6fdbeb401626',
    'wat-4fe0417bda474f7baa0e344b5c132778',
    'wat-af38a931039b45bb938b39f60b6bd697',

    -- Comprehensive Living Support
    'wat-fd76b4c2afad4eafae53d4c7dfc3dc84',
    'wat-98c4dcf5510047fe88c238e1fc35f0fa',
    'wat-0aa152d8533d454db8faf62d3e87d5e8'
);
