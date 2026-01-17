-- ============================================================================
-- 🚀 Script Name : Update wallet.wallet_type config_json using FOR loop
-- 📌 Purpose     : Iterates through predefined wallet label and wallet_type_code
--                combinations and conditionally adds or updates the
--                `"tag": "flex benefit"` entry inside `config_json`.
-- 👨‍💻 Author     : Srikanth Kodam
-- 📅 Date       : 2025-12-19
-- 🧾 Jira       : BEN-2326
-- 📤 Output :
--      - Updates `wallet.wallet_type.config_json`
--      - Adds or overwrites the `tag` attribute with value `flex benefit`
--      - Logs per-wallet update counts and final summary
-- 📝 Notes :
--      - Script processes only active records (`delete_nbr = 0`)
--      - Safe to re-run (idempotent; same value overwrites cleanly)
--      - Handles both NULL and non-NULL `config_json`
--      - Uses ROW_COUNT diagnostics for accurate update tracking
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
        SET config_json =
            CASE
                -- When config_json is NULL
                WHEN config_json IS NULL THEN
                    jsonb_build_object('tag', 'flex benefit')

                -- When config_json exists (add/update tag)
                ELSE
                    jsonb_set(
                        config_json::jsonb,
                        '{tag}',
                        '"flex benefit"',
                        true
                    )
            END
        WHERE delete_nbr = 0
          AND short_label = v_wallet_label
          AND wallet_type_code = v_wallet_type_code;

        -- Get updated row count for current iteration
        GET DIAGNOSTICS v_rows_updated = ROW_COUNT;
        v_total_updated := v_total_updated + v_rows_updated;

        -- Log per wallet type update
        RAISE NOTICE
            'Wallet Label: %, Wallet Type Code: %, Rows Updated: %',
            v_wallet_label,
            v_wallet_type_code,
            v_rows_updated;

    END LOOP;

    -- Final summary
    RAISE NOTICE 'Wallet Type config_json update completed';
    RAISE NOTICE '✅ Total Rows Updated: %', v_total_updated;

END;
$$;
