-- ============================================================================
-- 📌 Purpose   :
--   - Rollback script to revert multilingual `display_name` JSON structure
--     in `tenant.wallet_category.config_json` back to single-language format.
-- 🧑 Author    : Srikanth Kodam
-- 📅 Date      : 2025-11-06
-- 🧾 Jira      : BEN-1727
-- ⚠️ Inputs    :
--   - v_tenant_codes     : List of tenant codes (e.g., ARRAY['KP-TENANT-CODE'])
-- 📤 Output    :
--   - Reverts multilingual JSON back to single-language format.
-- 🔗 Script URL : <Optional Confluence / Documentation Link>
-- 📝 Notes     :
--   - Safe to re-run. Will simply reapply old format.
--   - Preserves all other keys (e.g., `api_source`, `google_type`, `initial_priority`).
-- ============================================================================

DO
$$
DECLARE
    v_tenant_codes      TEXT[] := ARRAY['KP-TENANT-CODE'];  -- Add multiple tenants if needed
    v_tenant_code       TEXT;
    v_wallet_type_code  TEXT := 'wat-a42e0b5cf3df4e0fbd431db58c415cad'; -- Rewards
    v_user_id           TEXT := 'SYSTEM';
    v_wallet_type_id    BIGINT;
    v_category_id       BIGINT;
    v_existing_json     JSONB;
    v_new_json          JSONB;
    v_api_source        TEXT := 'GOOGLE';
    v_updated_count     INT;
    rec                 RECORD;
BEGIN
    -- ✅ Loop through each tenant
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        v_updated_count := 0;

        -- Step 1️: Fetch wallet_type_id
        SELECT wallet_type_id
          INTO v_wallet_type_id
          FROM wallet.wallet_type
         WHERE wallet_type_code = v_wallet_type_code
           AND delete_nbr = 0
        LIMIT 1;

        IF v_wallet_type_id IS NULL THEN
            RAISE EXCEPTION '❌ Wallet type not found for code: %', v_wallet_type_code;
        END IF;

        -- Step 2️: Define data set for rollback
        FOR rec IN
            SELECT *
            FROM (VALUES
            ('car_repair','Car repair locations','Locales de reparación de automóviles'),
			('car_wash','car wash locations','locales de lavado de autos'),
			('electric_vehicle_charging_station','electric vehicle charging stations','estaciones de carga de vehículos eléctricos'),
			('gas_station','gas stations','estaciones de servicio'),
			('art_gallery','art galleries','galerías de arte'),
			('art_studio','art studios','estudios de arte'),
			('museum','museums','museos'),
			('performing_arts_theater','performing arts theaters','teatros de artes escénicas'),
			('adventure_sports_center','adventure sports centers','centros de deportes extremos'),
			('amphitheatre','amphitheaters','anfiteatros'),
			('amusement_center','amusement centers','centros de entretenimiento'),
			('amusement_park','amusement parks','parques de diversiones'),
			('aquarium','aquariums','acuarios'),
			('botanical_garden','botanical gardens','jardines botánicos'),
			('bowling_alley','bowling alleys','pistas de bolos'),
			('childrens_camp','children’s camps','campamentos infantiles'),
			('comedy_club','comedy clubs','clubes de comedia'),
			('concert_hall','concert halls','salas de conciertos'),
			('internet_cafe','internet cafes','cibercafés'),
			('movie_rental','movie rental locations','locales de alquiler de películas'),
			('movie_theater','movie theaters','salas de cine'),
			('opera_house','opera houses','teatros de ópera'),
			('philharmonic_hall','philharmonic halls','salas de conciertos filarmónicas'),
			('video_arcade','video arcades','salas de juegos de video'),
			('water_park','water parks','parques acuáticos'),
			('zoo','zoos','zoológicos'),
			('acai_shop','acai shops','tiendas de açaí'),
			('afghani_restaurant','afghani restaurants','restaurantes afganos'),
			('african_restaurant','african restaurants','restaurantes africanos'),
			('american_restaurant','american restaurants','restaurantes americanos'),
			('asian_restaurant','asian restaurants','restaurantes asiáticos'),
			('bagel_shop','bagel shops','tiendas de bagels'),
			('bakery','bakeries','panaderías'),
			('barbecue_restaurant','barbecue restaurants','restaurantes de barbacoa'),
			('brazilian_restaurant','brazilian restaurants','restaurantes brasileños'),
			('breakfast_restaurant','breakfast restaurants','restaurantes para desayunar'),
			('brunch_restaurant','brunch restaurants','restaurantes de brunch'),
			('buffet_restaurant','buffet restaurants','restaurantes bufé'),
			('cafe','cafes','cafés'),
			('candy_store','candy stores','tiendas de dulces'),
			('chinese_restaurant','chinese restaurants','restaurantes chinos'),
			('chocolate_factory','chocolate factories','fábricas de chocolate'),
			('chocolate_shop','chocolate shops','chocolaterías'),
			('coffee_shop','coffee shops','cafeterías'),
			('deli','deli','tienda de delicatessen'),
			('dessert_restaurant','dessert restaurants','restaurantes de repostería'),
			('dessert_shop','dessert shops','tiendas de repostería'),
			('diner','diners','comedores'),
			('donut_shop','donut shops','tiendas de donas'),
			('fast_food_restaurant','fast food restaurants','restaurantes de comida rápida'),
			('fine_dining_restaurant','fine dining restaurants','restaurantes gourmet'),
			('french_restaurant','french restaurants','restaurantes franceses'),
			('greek_restaurant','greek restaurants','restaurantes griegos'),
			('ice_cream_shop','ice cream shops','heladerías'),
			('indian_restaurant','indian restaurants','restaurantes indios'),
			('indonesian_restaurant','indonesian restaurants','restaurantes indonesios'),
			('italian_restaurant','italian restaurants','restaurantes italianos'),
			('japanese_restaurant','japanese restaurants','restaurantes japoneses'),
			('juice_shop','juice shops','tiendas de jugos'),
			('korean_restaurant','korean restaurants','restaurantes coreanos'),
			('lebanese_restaurant','lebanese restaurants','restaurantes libaneses'),
			('mediterranean_restaurant','mediterranean restaurants','restaurantes mediterráneos'),
			('mexican_restaurant','mexican restaurants','restaurantes mexicanos'),
			('middle_eastern_restaurant','middle eastern restaurants','restaurantes de medio oriente'),
			('pizza_restaurant','pizza restaurants','pizzerías'),
			('ramen_restaurant','ramen restaurants','restaurantes de ramen'),
			('restaurant','restaurants','restaurantes'),
			('sandwich_shop','sandwich shops','sandwicherías'),
			('seafood_restaurant','seafood restaurants','restaurantes de mariscos'),
			('spanish_restaurant','spanish restaurants','restaurantes españoles'),
			('steak_house','steak houses','restaurantes de carnes'),
			('sushi_restaurant','sushi restaurants','restaurantes de sushi'),
			('thai_restaurant','thai restaurants','restaurantes tailandeses'),
			('turkish_restaurant','turkish restaurants','restaurantes turcos'),
			('vegan_restaurant','vegan restaurants','restaurantes veganos'),
			('vegetarian_restaurant','vegetarian restaurants','restaurantes vegetarianos'),
			('vietnamese_restaurant','vietnamese restaurants','restaurantes vietnamitas'),
			('chiropractor','chiropractors','quiroprácticos'),
			('dental_clinic','dental clinics','clínicas dentales'),
			('dentist','dentists','dentistas'),
			('drugstore','drugstores','farmacias'),
			('massage','massage locations','centros de masajes'),
			('physiotherapist','physiotherapists','fisioterapeutas'),
			('sauna','saunas','saunas'),
			('skin_care_clinic','skin care clinics','clínicas de cuidado de la piel'),
			('spa','spas','spas'),
			('tanning_studio','tanning studios','salas de bronceado'),
			('wellness_center','wellness centers','centros de bienestar'),
			('yoga_studio','yoga studios','estudios de yoga'),
			('bed_and_breakfast','bed and breakfast locations','alojamientos con desayuno incluido'),
			('campground','campgrounds','sitios para acampar'),
			('cottage','cottages','cabañas'),
			('hotel','hotels','hoteles'),
			('inn','inns','posadas'),
			('motel','motels','moteles'),
			('resort_hotel','resort hotels','hoteles resort'),
			('rv_park','rv parks','parques de casas rodantes'),
			('barber_shop','barber shops','barberías'),
			('beautician','beauticians','estéticas'),
			('beauty_salon','beauty salons','salones de belleza'),
			('catering_service','catering services','servicios de catering'),
			('florist','florists','florerías'),
			('food_delivery','food delivery services','servicios de entrega de comida'),
			('foot_care','foot care locations','lugares para el cuidado de los pies'),
			('hair_care','hair care locations','lugares para el cuidado del cabello'),
			('hair_salon','hair salons','peluquerías'),
			('laundry','laundry services','servicios de lavandería'),
			('makeup_artist','makeup artists','artistas de maquillaje'),
			('moving_company','moving companies','empresas de mudanzas'),
			('nail_salon','nail salons','salones de manicura'),
			('psychic','psychics','videntes'),
			('storage','storage locations','depósitos'),
			('tailor','tailors','sastres'),
			('veterinary_care','veterinarians','veterinarios'),
			('asian_grocery_store','asian grocery stores','tiendas de comestibles asiáticas'),
			('auto_parts_store','auto parts stores','tiendas de repuestos automotriz'),
			('bicycle_store','bicycle stores','tiendas de bicicletas'),
			('book_store','book stores','librerías'),
			('butcher_shop','butcher shops','carnicerías'),
			('cell_phone_store','cell phone stores','tiendas de teléfonos celulares'),
			('clothing_store','clothing stores','tiendas de ropa'),
			('convenience_store','convenience stores','minimercado'),
			('department_store','department stores','grandes almacenes'),
			('discount_store','discount stores','tiendas de descuento'),
			('electronics_store','appliance stores','tiendas de electrodomésticos'),
			('food_store','food stores','tiendas de alimentos'),
			('furniture_store','furniture stores','mueblerías'),
			('gift_shop','gift shops','regalerías'),
			('grocery_store','grocery stores','tiendas de comestibles'),
			('hardware_store','hardware stores','ferreterías'),
			('home_goods_store','home goods stores','tiendas de artículos para el hogar'),
			('home_improvement_store','home improvement stores','tiendas de mejoras para el hogar'),
			('jewelry_store','jewelry stores','joyerías'),
			('market','markets','mercados'),
			('pet_store','pet stores','tiendas de mascotas'),
			('shoe_store','shoe stores','zapaterías'),
			('shopping_mall','shopping malls','centros comerciales'),
			('supermarket','supermarkets','supermercados'),
			('fishing_charter','fishing charters','alquiler de barcos de pesca'),
			('ice_skating_rink','ice skating rinks','pistas de patinaje sobre hielo'),
			('ski_resort','ski resorts','centros de esquí'),
			('stadium','stadiums','estadios')
            ) AS t(google_type, display_name_en, display_name_es)
        LOOP

            -- Step 3: Get category_id for google_type
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

            -- Step 4️: Fetch existing JSON
            SELECT config_json
              INTO v_existing_json
              FROM tenant.wallet_category wc
             WHERE wc.delete_nbr = 0
               AND wc.tenant_code = v_tenant_code
               AND wc.category_fk = v_category_id
               AND wc.wallet_type_id = v_wallet_type_id
               AND wc.wallet_type_code = v_wallet_type_code
            LIMIT 1;

            -- Step 5️: Build rollback JSON (old single-language format)
            v_new_json := jsonb_build_object(
                'api_source', v_api_source,
                'google_type', rec.google_type,
                'display_name', rec.display_name_en
            );

            -- Step 6️: Preserve initial_priority if exists
            IF v_existing_json ? 'initial_priority' THEN
                v_new_json := v_new_json || jsonb_build_object('initial_priority', v_existing_json->'initial_priority');
            END IF;

            -- Step 7️: Perform the update if existing record found
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

                RAISE NOTICE '♻️ Rolled back: tenant=% | category_id=% | google_type=%',
                             v_tenant_code, v_category_id, rec.google_type;
            END IF;

        END LOOP;

        -- Step 8️: Final Summary per tenant
        RAISE NOTICE '🎯 Rollback completed for tenant=% | Updated=%',
                     v_tenant_code, v_updated_count;

    END LOOP;
END;
$$;
