-- ============================================================================
-- 📌 Purpose   :
--   - Rollback script to simply update wallet_category rows
--     by setting delete_nbr = wallet_category_id.
--   - No JSON operations. No config_json updates.
-- 🧑 Author    : Srikanth Kodam
-- 📅 Date      : 2025-11-27
-- 🧾 Jira      : BEN-2228
-- ⚙️ Inputs :
--      v_tenant_codes      → Tenant identifiers (e.g., 'NAVITUS-TENANT-CODE')
-- 📤 Output :
--      - updates tenant.wallet_category_id entries
-- 🔗 Script URL : <Optional Confluence / Documentation Link>
-- ============================================================================

DO
$$
DECLARE
    v_tenant_codes      TEXT[] := ARRAY['NAVITUS-TENANT-CODE'];
    v_tenant_code       TEXT;

    v_wallet_type_code  TEXT := 'wat-cc96c9266dd543c3b1657d27d86adc0a'; -- Rewards
    v_wallet_type_id    BIGINT;

    v_user_id           TEXT := 'SYSTEM';

    v_category_id       BIGINT;
    v_wallet_category_id BIGINT;

    v_updated_count     INT;

    rec RECORD;
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        v_updated_count := 0;

        -- Get wallet type id
        SELECT wallet_type_id
          INTO v_wallet_type_id
          FROM wallet.wallet_type
         WHERE wallet_type_code = v_wallet_type_code
           AND delete_nbr = 0
        LIMIT 1;

        IF v_wallet_type_id IS NULL THEN
            RAISE EXCEPTION 'Wallet type not found for %', v_wallet_type_code;
        END IF;

        -- Defined rollback dataset (google_type)
        FOR rec IN
            SELECT *
            FROM (VALUES
                ('beauty_salon'),
				('bicycle_store'),
				('bowling_alley'),
				('clothing_store'),
				('convenience_store'),
				('courier_service'),
				('deli'),
				('drugstore'),
				('event_venue'),
				('fitness_center'),
				('food_store'),
				('golf_course'),
				('grocery_store'),
				('gym'),
				('hair_salon'),
				('locksmith'),
				('makeup_artist'),
				('marina'),
				('market'),
				('massage'),
				('pharmacy'),
				('sauna'),
				('shoe_store'),
				('ski_resort'),
				('skin_care_clinic'),
				('spa'),
				('sporting_goods_store'),
				('sports_club'),
				('sports_complex'),
				('stable'),
				('supermarket'),
				('swimming_pool'),
				('tanning_studio'),
				('telecommunications_service_provider'),
				('tourist_information_center'),
				('warehouse_store'),
				('wedding_venue'),
				('wellness_center'),
				('wholesaler'),
				('yoga_studio')
            ) AS t(google_type)
        LOOP

            -- Fetch category
            SELECT id
              INTO v_category_id
              FROM tenant.category
             WHERE is_active = TRUE
               AND delete_nbr = 0
               AND google_type = rec.google_type
            LIMIT 1;

            IF v_category_id IS NULL THEN
                RAISE NOTICE 'Skipping google_type=% — no category found', rec.google_type;
                CONTINUE;
            END IF;

            -- Find wallet_category record
            SELECT wallet_category_id
              INTO v_wallet_category_id
              FROM tenant.wallet_category
             WHERE delete_nbr = 0
               AND tenant_code = v_tenant_code
               AND category_fk = v_category_id
               AND wallet_type_id = v_wallet_type_id
               AND wallet_type_code = v_wallet_type_code
            LIMIT 1;

            IF v_wallet_category_id IS NULL THEN
                CONTINUE;
            END IF;

            -- Rollback update ONLY using wallet_category_id
            UPDATE tenant.wallet_category
               SET delete_nbr = v_wallet_category_id,
                   update_ts = NOW(),
                   update_user = v_user_id
             WHERE wallet_category_id = v_wallet_category_id;

            v_updated_count := v_updated_count + 1;

            RAISE NOTICE 'Rolled back → tenant=% | category_id=% | wallet_category_id=%',
                         v_tenant_code, v_category_id, v_wallet_category_id;

        END LOOP;

        RAISE NOTICE 'Rollback completed for tenant=% | Updated=%',
                     v_tenant_code, v_updated_count;

    END LOOP;
END;
$$;
