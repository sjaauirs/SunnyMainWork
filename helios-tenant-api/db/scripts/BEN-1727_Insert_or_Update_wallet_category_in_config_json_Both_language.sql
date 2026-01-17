-- ============================================================================
-- 🚀 Script    : Insert or Update tenant.wallet_category JSON with multilingual data
-- 📌 Purpose   : Ensures each configured google_type has a wallet category entry 
--               for the specified tenant and wallet type. Converts old single-language
--               JSON format into new multilingual format (English + Spanish).
-- 👨‍💻 Author   : Srikanth Kodam
-- 📅 Date      : 2025-11-06
-- 🧾 Jira      : BEN-1727
-- ⚙️ Inputs :
--      v_tenant_code        → Tenant identifier (e.g., '<KP-TENANT-CODE>')
-- 📤 Output :
--      - Inserts or updates tenant.wallet_category entries
--      - Adds multilingual display_name in config_json
--      - Preserves initial_priority if defined
--      - Logs inserted / updated records for verification
-- 🔗 Script URL : <Optional Confluence / Documentation Link>
-- 📝 Notes :
--      - Script is idempotent (safe to re-run)
--      - Skips google_type without a matching active tenant.category
--      - Automatically handles insert/update with audit fields
-- ============================================================================

DO
$$
DECLARE
    v_tenant_codes TEXT[] := ARRAY['KP-TENANT-CODE']; 
    v_tenant_code       TEXT;
    v_wallet_type_code  TEXT := 'wat-a42e0b5cf3df4e0fbd431db58c415cad'; -- Rewards
    v_user_id           TEXT := 'SYSTEM';
    v_wallet_type_id    BIGINT;
    v_category_id       BIGINT;
    v_existing_json     JSONB;
    v_new_json          JSONB;
    v_api_source        TEXT := 'GOOGLE';
    v_inserted_count    INT;
    v_updated_count     INT;
    v_priority          INT;
    rec                 RECORD;
BEGIN
    -- ✅ Loop through each tenant
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

        -- Step 2: Define data set
        FOR rec IN
            SELECT *
            FROM (VALUES
            (0,'car_repair','Car repair locations','Locales de reparación de automóviles'),
			(0,'car_wash','Car wash locations','Locales de lavado de autos'),
			(0,'electric_vehicle_charging_station','Electric vehicle charging stations','Estaciones de carga de vehículos eléctricos'),
			(0,'gas_station','Gas stations','Estaciones de servicio'),
			(0,'art_gallery','Art galleries','Galerías de arte'),
			(0,'art_studio','Art studios','Estudios de arte'),
			(0,'museum','Museums','Museos'),
			(0,'performing_arts_theater','Performing arts theaters','Teatros de artes escénicas'),
			(0,'adventure_sports_center','Adventure sports centers','Centros de deportes extremos'),
			(0,'amphitheatre','Amphitheaters','Anfiteatros'),
			(0,'amusement_center','Amusement centers','Centros de entretenimiento'),
			(0,'amusement_park','Amusement parks','Parques de diversiones'),
			(0,'aquarium','Aquariums','Acuarios'),
			(0,'botanical_garden','Botanical gardens','Jardines botánicos'),
			(0,'bowling_alley','Bowling alleys','Pistas de bolos'),
			(0,'childrens_camp','Children’s camps','Campamentos infantiles'),
			(0,'comedy_club','Comedy clubs','Clubes de comedia'),
			(0,'concert_hall','Concert halls','Salas de conciertos'),
			(0,'internet_cafe','Internet cafes','Cibercafés'),
			(0,'movie_rental','Movie rental locations','Locales de alquiler de películas'),
			(4,'movie_theater','Movie theaters','Salas de cine'),
			(0,'opera_house','Opera houses','Teatros de ópera'),
			(0,'philharmonic_hall','Philharmonic halls','Salas de conciertos filarmónicas'),
			(0,'video_arcade','Video arcades','Salas de juegos de video'),
			(0,'water_park','Water parks','Parques acuáticos'),
			(0,'zoo','Zoos','Zoológicos'),
			(0,'acai_shop','Acai shops','Tiendas de açaí'),
			(0,'afghani_restaurant','Afghani restaurants','Restaurantes afganos'),
			(0,'african_restaurant','African restaurants','Restaurantes africanos'),
			(0,'american_restaurant','American restaurants','Restaurantes americanos'),
			(0,'asian_restaurant','Asian restaurants','Restaurantes asiáticos'),
			(0,'bagel_shop','Bagel shops','Tiendas de bagels'),
			(0,'bakery','Bakeries','Panaderías'),
			(0,'barbecue_restaurant','Barbecue restaurants','Restaurantes de barbacoa'),
			(0,'brazilian_restaurant','Brazilian restaurants','Restaurantes brasileños'),
			(0,'breakfast_restaurant','Breakfast restaurants','Restaurantes para desayunar'),
			(0,'brunch_restaurant','Brunch restaurants','Restaurantes de brunch'),
			(0,'buffet_restaurant','Buffet restaurants','Restaurantes bufé'),
			(0,'cafe','Cafes','Cafés'),
			(0,'candy_store','Candy stores','Tiendas de dulces'),
			(0,'chinese_restaurant','Chinese restaurants','Restaurantes chinos'),
			(0,'chocolate_factory','Chocolate factories','Fábricas de chocolate'),
			(0,'chocolate_shop','Chocolate shops','Chocolaterías'),
			(0,'coffee_shop','Coffee shops','Cafeterías'),
			(0,'deli','Deli','Tienda de delicatessen'),
			(0,'dessert_restaurant','Dessert restaurants','Restaurantes de repostería'),
			(0,'dessert_shop','Dessert shops','Tiendas de repostería'),
			(0,'diner','Diners','Comedores'),
			(0,'donut_shop','Donut shops','Tiendas de donas'),
			(0,'fast_food_restaurant','Fast food restaurants','Restaurantes de comida rápida'),
			(0,'fine_dining_restaurant','Fine dining restaurants','Restaurantes gourmet'),
			(0,'french_restaurant','French restaurants','Restaurantes franceses'),
			(0,'greek_restaurant','Greek restaurants','Restaurantes griegos'),
			(0,'ice_cream_shop','Ice cream shops','Heladerías'),
			(0,'indian_restaurant','Indian restaurants','Restaurantes indios'),
			(0,'indonesian_restaurant','Indonesian restaurants','Restaurantes indonesios'),
			(0,'italian_restaurant','Italian restaurants','Restaurantes italianos'),
			(0,'japanese_restaurant','Japanese restaurants','Restaurantes japoneses'),
			(0,'juice_shop','Juice shops','Tiendas de jugos'),
			(0,'korean_restaurant','Korean restaurants','Restaurantes coreanos'),
			(0,'lebanese_restaurant','Lebanese restaurants','Restaurantes libaneses'),
			(0,'mediterranean_restaurant','Mediterranean restaurants','Restaurantes mediterráneos'),
			(0,'mexican_restaurant','Mexican restaurants','Restaurantes mexicanos'),
			(0,'middle_eastern_restaurant','Middle eastern restaurants','Restaurantes de Medio Oriente'),
			(0,'pizza_restaurant','Pizza restaurants','Pizzerías'),
			(0,'ramen_restaurant','Ramen restaurants','Restaurantes de ramen'),
			(0,'restaurant','Restaurants','Restaurantes'),
			(0,'sandwich_shop','Sandwich shops','Sandwicherías'),
			(0,'seafood_restaurant','Seafood restaurants','Restaurantes de mariscos'),
			(0,'spanish_restaurant','Spanish restaurants','Restaurantes españoles'),
			(0,'steak_house','Steak houses','Restaurantes de carnes'),
			(0,'sushi_restaurant','Sushi restaurants','Restaurantes de sushi'),
			(0,'thai_restaurant','Thai restaurants','Restaurantes tailandeses'),
			(0,'turkish_restaurant','Turkish restaurants','Restaurantes turcos'),
			(0,'vegan_restaurant','Vegan restaurants','Restaurantes veganos'),
			(0,'vegetarian_restaurant','Vegetarian restaurants','Restaurantes vegetarianos'),
			(0,'vietnamese_restaurant','Vietnamese restaurants','Restaurantes vietnamitas'),
			(0,'chiropractor','Chiropractors','Quiroprácticos'),
			(0,'dental_clinic','Dental clinics','Clínicas dentales'),
			(0,'dentist','Dentists','Dentistas'),
			(0,'drugstore','Drugstores','Farmacias'),
			(0,'massage','Massage locations','Centros de masajes'),
			(0,'physiotherapist','Physiotherapists','Fisioterapeutas'),
			(0,'sauna','Saunas','Saunas'),
			(0,'skin_care_clinic','Skin care clinics','Clínicas de cuidado de la piel'),
			(0,'spa','Spas','Spas'),
			(0,'tanning_studio','Tanning studios','Salas de bronceado'),
			(0,'wellness_center','Wellness centers','Centros de bienestar'),
			(0,'yoga_studio','Yoga studios','Estudios de yoga'),
			(0,'bed_and_breakfast','Bed and breakfast locations','Alojamientos con desayuno incluido'),
			(0,'campground','Campgrounds','Sitios para acampar'),
			(0,'cottage','Cottages','Cabañas'),
			(0,'hotel','Hotels','Hoteles'),
			(0,'inn','Inns','Posadas'),
			(0,'motel','Motels','Moteles'),
			(0,'resort_hotel','Resort hotels','Hoteles resort'),
			(0,'rv_park','RV parks','Parques de casas rodantes'),
			(0,'barber_shop','Barber shops','Barberías'),
			(0,'beautician','Beauticians','Estéticas'),
			(0,'beauty_salon','Beauty salons','Salones de belleza'),
			(0,'catering_service','Catering services','Servicios de catering'),
			(0,'florist','Florists','Florerías'),
			(0,'food_delivery','Food delivery services','Servicios de entrega de comida'),
			(0,'foot_care','Foot care locations','Lugares para el cuidado de los pies'),
			(0,'hair_care','Hair care locations','Lugares para el cuidado del cabello'),
			(3,'hair_salon','Hair salons','Peluquerías'),
			(0,'laundry','Laundry services','Servicios de lavandería'),
			(0,'makeup_artist','Makeup artists','Artistas de maquillaje'),
			(0,'moving_company','Moving companies','Empresas de mudanzas'),
			(0,'nail_salon','Nail salons','Salones de manicura'),
			(0,'psychic','Psychics','Videntes'),
			(0,'storage','Storage locations','Depósitos'),
			(0,'tailor','Tailors','Sastres'),
			(0,'veterinary_care','Veterinarians','Veterinarios'),
			(0,'asian_grocery_store','Asian grocery stores','Tiendas de comestibles asiáticas'),
			(0,'auto_parts_store','Auto parts stores','Tiendas de repuestos automotriz'),
			(0,'bicycle_store','Bicycle stores','Tiendas de bicicletas'),
			(0,'book_store','Book stores','Librerías'),
			(0,'butcher_shop','Butcher shops','Carnicerías'),
			(0,'cell_phone_store','Cell phone stores','Tiendas de teléfonos celulares'),
			(0,'clothing_store','Clothing stores','Tiendas de ropa'),
			(0,'convenience_store','Convenience stores','Minimercado'),
			(1,'department_store','Department stores','Grandes almacenes'),
			(0,'discount_store','Discount stores','Tiendas de descuento'),
			(0,'electronics_store','Appliance stores','Tiendas de electrodomésticos'),
			(0,'food_store','Food stores','Tiendas de alimentos'),
			(0,'furniture_store','Furniture stores','Mueblerías'),
			(2,'gift_shop','Gift shops','Regalerías'),
			(0,'grocery_store','Grocery stores','Tiendas de comestibles'),
			(0,'hardware_store','Hardware stores','Ferreterías'),
			(0,'home_goods_store','Home goods stores','Tiendas de artículos para el hogar'),
			(0,'home_improvement_store','Home improvement stores','Tiendas de mejoras para el hogar'),
			(0,'jewelry_store','Jewelry stores','Joyerías'),
			(0,'market','Markets','Mercados'),
			(0,'pet_store','Pet stores','Tiendas de mascotas'),
			(0,'shoe_store','Shoe stores','Zapaterías'),
			(0,'shopping_mall','Shopping malls','Centros comerciales'),
			(0,'supermarket','Supermarkets','Supermercados'),
			(0,'fishing_charter','Fishing charters','Alquiler de barcos de pesca'),
			(0,'ice_skating_rink','Ice skating rinks','Pistas de patinaje sobre hielo'),
			(0,'ski_resort','Ski resorts','Centros de esquí'),
			(0,'stadium','Stadiums','Estadios')
            ) AS t(initial_priority,google_type, display_name_en, display_name_es)
        LOOP

            -- Step 2.1: Get category_id
            SELECT id
              INTO v_category_id
              FROM tenant.category
             WHERE is_active = true
               AND delete_nbr = 0
               AND google_type = rec.google_type
            LIMIT 1;

            IF v_category_id IS NULL THEN
                RAISE NOTICE '⚠️ Skipping google_type=% — no active tenant.category found', rec.google_type;
                CONTINUE;
            END IF;

            -- Step 2.3: Check if record exists
            SELECT config_json
              INTO v_existing_json
              FROM tenant.wallet_category wc
             WHERE wc.delete_nbr = 0
               AND wc.tenant_code = v_tenant_code
               AND wc.category_fk = v_category_id
               AND wc.wallet_type_id = v_wallet_type_id
               AND wc.wallet_type_code = v_wallet_type_code
            LIMIT 1;

            -- Step 2.4: Build new multilingual JSON
            v_new_json := jsonb_build_object(
                'api_source', v_api_source,
                'google_type', rec.google_type,
                'display_name', jsonb_build_object(
                    'en-US', rec.display_name_en,
                    'es', rec.display_name_es
                )
            );

            -- Step 2.5: Add initial_priority if > 0
            IF rec.initial_priority > 0 THEN
                v_new_json := v_new_json || jsonb_build_object('initial_priority', rec.initial_priority);
            END IF;

            -- Step 2.6: Update or Insert
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
                RAISE NOTICE '🔄 Updated: tenant=% category_id=% google_type=%',
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
                RAISE NOTICE '✅ Inserted: tenant=% category_id=% google_type=%',
                             v_tenant_code, v_category_id, rec.google_type;
            END IF;

        END LOOP;

        -- Step 3: Final Summary for this tenant
        RAISE NOTICE '🎯 Completed for tenant=% | Inserted=% | Updated=%',
                     v_tenant_code, v_inserted_count, v_updated_count;

    END LOOP;
END;
$$;
