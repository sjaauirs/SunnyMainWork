-- ============================================================================
-- 🚀 Script Name : Rollback – Remove `tag` from wallet.wallet_type config_json
-- 📌 Purpose    : Rolls back the previously applied configuration change by
--                removing the `"tag"` key from `config_json` for selected
--                wallet types.
-- 👨‍💻 Author     : Srikanth Kodam
-- 📅 Date       : 2025-12-19
-- 🧾 Jira       : BEN-2326
-- 📤 Output :
--      - Updates `wallet.wallet_type.config_json`
--      - Removes the `tag` key where present
--      - Logs per-wallet rollback counts and final summary
-- 📝 Notes :
--      - Script processes only active records (`delete_nbr = 0`)
--      - Safe to re-run (idempotent; no effect if `tag` is already absent)
--      - Uses JSONB `-` operator to remove a single key
--      - Tracks updates using ROW_COUNT diagnostics
-- ============================================================================

DO
$$
DECLARE
    v_wallet_label        TEXT;
    v_wallet_type_code    TEXT;
    v_rows_updated        INT;
    v_total_updated       INT := 0;
BEGIN
    RAISE NOTICE 'Wallet Type config_json update process started';

    -- Loop through wallet label & wallet_type_code pairs
    FOR v_wallet_label, v_wallet_type_code IN
        SELECT *
        FROM (VALUES
            ('OTC','wat-4b364fg722f04034cv732b355d84f479'),
            ('OTC','wat-4b364ed612f04034bf732b355d84f368'),
            ('UGT','wat-e207db6a8a0a460fbe852ce9c3fcbd54'),
            ('UGT','wat-49812db3d9814dbca8eae2eba91722af'),
            ('DOT','wat-5b0d5378af774c0381b67ed3e77d2fdd'),
            ('DOT','wat-7ab9caa63bb14a6093649fbf3b97b0b4'),
            ('HFC','wat-98c4dcf5510047fe88c238e1fc35f0fa'),
            ('HFC','wat-fd76b4c2afad4eafae53d4c7dfc3dc84'),
            ('CFO','wat-3509a5788e5246b18221582031cd10a3'),
            ('CFO','wat-bb06d4c12ac84213bc59bc2093421264'),
            ('OCP','wat-14cfd51de64c46e4b927a7e8984474ea'),
            ('OCP','wat-7be788fb5115443eb0ead237b6c46cc4'),
            ('OGP','wat-2422da2eb57b4a2c9acb24e4d593fba7'),
            ('OGP','wat-aca6aa177739432980e094b86567db7d'),
            ('OGT','wat-c583162a9130457289a09e28daaedc2e'),
            ('OGT','wat-2ea762719bac47349aac36e7b2ade583'),
            ('HFO','wat-4fe0417bda474f7baa0e344b5c132778'),
            ('HFO','wat-44b999834ec344c88c1f6fdbeb401626')
        ) AS t(wallet_label, wallet_type_code)
    LOOP

        -- Update config_json for each wallet type
        UPDATE wallet.wallet_type
		SET config_json = config_json::jsonb - 'tag'
		WHERE delete_nbr = 0
		  AND config_json IS NOT NULL
		  AND config_json::jsonb ? 'tag'
		  AND short_label = v_wallet_label
          AND wallet_type_code = v_wallet_type_code;

        -- Get rollbacked row count for current iteration
        GET DIAGNOSTICS v_rows_updated = ROW_COUNT;
        v_total_updated := v_total_updated + v_rows_updated;

        -- Log per wallet type update
        RAISE NOTICE
            'Wallet Label: %, Wallet Type Code: %, Rows rollbacked: %',
            v_wallet_label,
            v_wallet_type_code,
            v_rows_updated;

    END LOOP;

    -- Final summary
    RAISE NOTICE 'Wallet Type config_json update completed';
    RAISE NOTICE '✅ Total Rows rollbacked: %', v_total_updated;

END;
$$;