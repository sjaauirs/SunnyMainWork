-- ============================================================================
-- 🚀 Script    : Insert or Update tenant.wallet_category JSON with multilingual data
-- 📌 Purpose   : Ensures each configured google_type has a wallet category entry 
--                for the specified tenant and wallet type. Converts old single-language
--                JSON format into new multilingual format (English & Spanish).
-- 👨‍💻 Author   : Srikanth Kodam
-- 📅 Date      : 2025-11-27
-- 🧾 Jira      : BEN-2228
-- ⚙️ Inputs :
--      v_tenant_codes      → Tenant identifiers (e.g., 'NAVITUS-TENANT-CODE')
-- 📤 Output :
--      - Inserts or updates tenant.wallet_category entries
--      - Adds multilingual display_name in config_json
--      - Preserves initial_priority when provided
--      - Logs inserted / updated records for verification
-- 🔗 Script URL : <Optional Confluence / Documentation Link>
-- 📝 Notes :
--      - Script is idempotent (safe to re-run)
--      - Creates tenant.category entries if missing
--      - Automatically handles audit fields
-- ============================================================================

DO
$$
DECLARE
    v_tenant_codes       TEXT[] := ARRAY['NAVITUS-TENANT-CODE'];
    v_tenant_code        TEXT;
    v_wallet_type_code   TEXT := 'wat-cc96c9266dd543c3b1657d27d86adc0a'; -- Rewards
    v_wallet_type_id     BIGINT;
    v_user_id            TEXT := 'SYSTEM';
    v_category_id        BIGINT;
    v_existing_json      JSONB;
    v_new_json           JSONB;
    v_api_source         TEXT := 'GOOGLE';
    v_inserted_count     INT;
    v_updated_count      INT;
    rec                  RECORD;
BEGIN
    -- Loop through each tenant
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        v_inserted_count := 0;
        v_updated_count := 0;

        -- Step 1: Fetch wallet_type_id
        SELECT wallet_type_id
          INTO v_wallet_type_id
          FROM wallet.wallet_type
         WHERE wallet_type_code = v_wallet_type_code
           AND delete_nbr = 0
        LIMIT 1;

        IF v_wallet_type_id IS NULL THEN
            RAISE EXCEPTION '❌ Wallet type not found for code: %', v_wallet_type_code;
        END IF;

        -- Step 2: Data Set (google_type, display_name_en, display_name_es)
        FOR rec IN
            SELECT *
            FROM (VALUES
                ('0','beauty_salon','Beauty salons',''),
				('0','bicycle_store','Bicycle stores',''),
				('0','bowling_alley','Bowling alleys',''),
				('0','clothing_store','Clothing stores',''),
				('0','convenience_store','Convenience stores',''),
				('0','courier_service','Courier services',''),
				('0','deli','Delis',''),
				('0','drugstore','Drugstores',''),
				('0','event_venue','Event venues',''),
				('3','fitness_center','Fitness centers',''),
				('0','food_store','Food stores',''),
				('0','golf_course','Golf courses',''),
				('0','grocery_store','Grocery stores',''),
				('0','gym','Gyms',''),
				('2','hair_salon','Hair salons',''),
				('0','locksmith','Locksmiths',''),
				('0','makeup_artist','Makeup artists',''),
				('0','marina','Marinas',''),
				('0','market','Markets',''),
				('0','massage','Massage services',''),
				('0','pharmacy','Pharmacies',''),
				('0','sauna','Saunas',''),
				('0','shoe_store','Shoe stores',''),
				('0','ski_resort','Ski Resort',''),
				('0','skin_care_clinic','Skin care clinics',''),
				('0','spa','Spas',''),
				('4','sporting_goods_store','Sporting goods stores',''),
				('0','sports_club','Sports clubs',''),
				('0','sports_complex','Sports complexes',''),
				('0','stable','Stables',''),
				('1','supermarket','Supermarkets',''),
				('0','swimming_pool','Swimming pools',''),
				('0','tanning_studio','Tanning studios',''),
				('0','telecommunications_service_provider','Telecommunications service providers',''),
				('0','tourist_information_center','Tourist information centers',''),
				('0','warehouse_store','Warehouse stores',''),
				('0','wedding_venue','Wedding venues',''),
				('0','wellness_center','Wellness centers',''),
				('0','wholesaler','Wholesalers',''),
				('0','yoga_studio','Yoga studios','')
            ) AS t(initial_priority, google_type, display_name_en, display_name_es)
        LOOP

            -- Step 2.1: Fetch or Create tenant.category
            SELECT id
              INTO v_category_id
              FROM tenant.category
             WHERE is_active = TRUE
               AND delete_nbr = 0
               AND google_type = rec.google_type
            LIMIT 1;

            -- Create category if missing
            IF v_category_id IS NULL THEN
                INSERT INTO tenant.category (
                    name,
                    google_type,
                    is_active,
                    create_ts,
                    create_user,
                    delete_nbr
                )
                VALUES (
                    rec.display_name_en,
                    rec.google_type,
                    TRUE,
                    NOW(),
                    v_user_id,
                    0
                );
            END IF;

            -- Re-fetch category id
            SELECT id
              INTO v_category_id
              FROM tenant.category
             WHERE is_active = TRUE
               AND delete_nbr = 0
               AND google_type = rec.google_type
            LIMIT 1;

            IF v_category_id IS NULL THEN
                RAISE NOTICE '⚠️ Skipping google_type=% — no active tenant.category found', rec.google_type;
                CONTINUE;
            END IF;

            -- Step 2.3: Check existing wallet_category
            SELECT config_json
              INTO v_existing_json
              FROM tenant.wallet_category wc
             WHERE wc.delete_nbr = 0
               AND wc.tenant_code = v_tenant_code
               AND wc.category_fk = v_category_id
               AND wc.wallet_type_id = v_wallet_type_id
               AND wc.wallet_type_code = v_wallet_type_code
            LIMIT 1;

            -- Step 2.4: Build multilingual JSON
            v_new_json := jsonb_build_object(
                'api_source', v_api_source,
                'google_type', rec.google_type,
                'display_name', jsonb_build_object(
                    'en-US', rec.display_name_en,
                    'es',    rec.display_name_es
                )
            );

            -- Step 2.5: Add initial_priority if > 0
            IF rec.initial_priority <> '0' THEN
                v_new_json := v_new_json || jsonb_build_object('initial_priority', rec.initial_priority);
            END IF;

            -- Step 2.6: Insert or Update
            IF v_existing_json IS NOT NULL THEN
                UPDATE tenant.wallet_category wc
                   SET config_json = v_new_json,
                       update_ts   = NOW(),
                       update_user = v_user_id
                 WHERE wc.delete_nbr = 0
                   AND wc.tenant_code = v_tenant_code
                   AND wc.category_fk = v_category_id
                   AND wc.wallet_type_id = v_wallet_type_id
                   AND wc.wallet_type_code = v_wallet_type_code;

                v_updated_count := v_updated_count + 1;

                RAISE NOTICE '🔄 Updated → tenant=% | category_id=% | google_type=%',
                             v_tenant_code, v_category_id, rec.google_type;
            ELSE
                INSERT INTO tenant.wallet_category (
                    wallet_type_id,
                    tenant_code,
                    wallet_type_code,
                    category_fk,
                    config_json,
                    create_ts,
                    create_user,
                    delete_nbr
                )
                VALUES (
                    v_wallet_type_id,
                    v_tenant_code,
                    v_wallet_type_code,
                    v_category_id,
                    v_new_json,
                    NOW(),
                    v_user_id,
                    0
                );

                v_inserted_count := v_inserted_count + 1;

                RAISE NOTICE '✅ Inserted → tenant=% | category_id=% | google_type=%',
                             v_tenant_code, v_category_id, rec.google_type;
            END IF;

        END LOOP;

        -- Step 3: Summary
        RAISE NOTICE '🎯 Completed tenant=% | Inserted=% | Updated=%',
                     v_tenant_code, v_inserted_count, v_updated_count;

    END LOOP;
END;
$$;
