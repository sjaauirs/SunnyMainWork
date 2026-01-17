-- ============================================================================
-- 🚀 Script    : Add wallet_category and category tables to tenant schema
-- 📌 Purpose   : Add wallet_category and category tables to tenant schema
-- 🧑 Author    : Preeti
-- 📅 Date      : 25/09/2025
-- 🧾 Jira      : BEN-249
-- ⚠️ Inputs    : HAP-TENANT-CODE
-- ============================================================================

-- Create category table
CREATE TABLE tenant.category (
    id SERIAL PRIMARY KEY,
    name varchar NOT NULL,
    google_type TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    create_ts TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    update_ts TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    create_user TEXT,
    update_user TEXT,
    delete_nbr INT DEFAULT 0
);
 
GRANT DELETE, INSERT, SELECT, UPDATE ON TABLE tenant.category TO happusr; 
GRANT SELECT ON TABLE tenant.category TO hrousr; 
GRANT ALL ON TABLE tenant.category TO hschupdusr; 
GRANT ALL ON TABLE tenant.category TO hadminusr;

-- Create wallet_category table
create table tenant.wallet_category (
    id SERIAL PRIMARY KEY,
    tenant_code varchar NOT NULL,
    wallet_type_id BIGINT NOT NULL REFERENCES wallet.wallet_type(wallet_type_id),
    wallet_type_code varchar NOT NULL REFERENCES wallet.wallet_type(wallet_type_code),
    category_fk INT NOT NULL REFERENCES tenant.category(id),
    config_json JSONB,
    create_ts TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    update_ts TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    create_user TEXT,
    update_user TEXT,
    delete_nbr INT DEFAULT 0
);

GRANT DELETE, INSERT, SELECT, UPDATE ON TABLE tenant.wallet_category TO happusr; 
GRANT SELECT ON TABLE tenant.wallet_category TO hrousr; 
GRANT ALL ON TABLE tenant.wallet_category TO hschupdusr; 
GRANT ALL ON TABLE tenant.wallet_category TO hadminusr;

-- Insert values into category table
INSERT INTO tenant.category (id, name, google_type, is_active, create_user, update_user)
VALUES
    (1, 'Car repair locations', 'car_repair', TRUE, 'SYSTEM', 'SYSTEM'),
    (2, 'Car wash locations', 'car_wash', TRUE, 'SYSTEM', 'SYSTEM'),
    (3, 'Electric vehicle charging stations', 'electric_vehicle_charging_station', TRUE, 'SYSTEM', 'SYSTEM'),
    (4, 'Gas stations', 'gas_station', TRUE, 'SYSTEM', 'SYSTEM'),
    (5, 'Art galleries', 'art_gallery', TRUE, 'SYSTEM', 'SYSTEM'),
    (6, 'Art studios', 'art_studio', TRUE, 'SYSTEM', 'SYSTEM'),
    (7, 'Museums', 'museum', TRUE, 'SYSTEM', 'SYSTEM'),
    (8, 'Performing arts theaters', 'performing_arts_theater', TRUE, 'SYSTEM', 'SYSTEM'),
    (9, 'Adventure sports centers', 'adventure_sports_center', TRUE, 'SYSTEM', 'SYSTEM'),
    (10, 'Amphitheaters', 'amphitheatre', TRUE, 'SYSTEM', 'SYSTEM'),
    (11, 'Amusement centers', 'amusement_center', TRUE, 'SYSTEM', 'SYSTEM'),
    (12, 'Amusement parks', 'amusement_park', TRUE, 'SYSTEM', 'SYSTEM'),
    (13, 'Aquariums', 'aquarium', TRUE, 'SYSTEM', 'SYSTEM'),
    (14, 'Botanical gardens', 'botanical_garden', TRUE, 'SYSTEM', 'SYSTEM'),
    (15, 'Bowling alleys', 'bowling_alley', TRUE, 'SYSTEM', 'SYSTEM'),
    (16, 'Children''s camps', 'childrens_camp', TRUE, 'SYSTEM', 'SYSTEM'),
    (17, 'Comedy clubs', 'comedy_club', TRUE, 'SYSTEM', 'SYSTEM'),
    (18, 'Concert halls', 'concert_hall', TRUE, 'SYSTEM', 'SYSTEM'),
    (19, 'Internet cafes', 'internet_cafe', TRUE, 'SYSTEM', 'SYSTEM'),
    (20, 'Movie rental locations', 'movie_rental', TRUE, 'SYSTEM', 'SYSTEM'),
    (21, 'Movie theaters', 'movie_theater', TRUE, 'SYSTEM', 'SYSTEM'),
    (22, 'Opera houses', 'opera_house', TRUE, 'SYSTEM', 'SYSTEM'),
    (23, 'Philharmonic halls', 'philharmonic_hall', TRUE, 'SYSTEM', 'SYSTEM'),
    (24, 'Video arcades', 'video_arcade', TRUE, 'SYSTEM', 'SYSTEM'),
    (25, 'Water parks', 'water_park', TRUE, 'SYSTEM', 'SYSTEM'),
    (26, 'Zoos', 'zoo', TRUE, 'SYSTEM', 'SYSTEM'),
    (27, 'Acai shops', 'acai_shop', TRUE, 'SYSTEM', 'SYSTEM'),
    (28, 'Afghani restaurants', 'afghani_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (29, 'African restaurants', 'african_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (30, 'American restaurants', 'american_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (31, 'Asian restaurants', 'asian_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (32, 'Bagel shops', 'bagel_shop', TRUE, 'SYSTEM', 'SYSTEM'),
    (33, 'Bakeries', 'bakery', TRUE, 'SYSTEM', 'SYSTEM'),
    (34, 'Barbecue restaurants', 'barbecue_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (35, 'Brazilian restaurants', 'brazilian_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (36, 'Breakfast restaurants', 'breakfast_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (37, 'Brunch restaurants', 'brunch_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (38, 'Buffet restaurants', 'buffet_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (39, 'Cafes', 'cafe', TRUE, 'SYSTEM', 'SYSTEM'),
    (40, 'Candy stores', 'candy_store', TRUE, 'SYSTEM', 'SYSTEM'),
    (41, 'Chinese restaurants', 'chinese_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (42, 'Chocolate factories', 'chocolate_factory', TRUE, 'SYSTEM', 'SYSTEM'),
    (43, 'Chocolate shops', 'chocolate_shop', TRUE, 'SYSTEM', 'SYSTEM'),
    (44, 'Coffee shops', 'coffee_shop', TRUE, 'SYSTEM', 'SYSTEM'),
    (45, 'Delis', 'deli', TRUE, 'SYSTEM', 'SYSTEM'),
    (46, 'Dessert restaurants', 'dessert_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (47, 'Dessert shops', 'dessert_shop', TRUE, 'SYSTEM', 'SYSTEM'),
    (48, 'Diners', 'diner', TRUE, 'SYSTEM', 'SYSTEM'),
    (49, 'Donut shops', 'donut_shop', TRUE, 'SYSTEM', 'SYSTEM'),
    (50, 'Fast food restaurants', 'fast_food_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (51, 'Fine dining restaurants', 'fine_dining_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (52, 'French restaurants', 'french_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (53, 'Greek restaurants', 'greek_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (54, 'Ice cream shops', 'ice_cream_shop', TRUE, 'SYSTEM', 'SYSTEM'),
    (55, 'Indian restaurants', 'indian_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (56, 'Indonesian restaurants', 'indonesian_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (57, 'Italian restaurants', 'italian_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (58, 'Japanese restaurants', 'japanese_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (59, 'Juice shops', 'juice_shop', TRUE, 'SYSTEM', 'SYSTEM'),
    (60, 'Korean restaurants', 'korean_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (61, 'Lebanese restaurants', 'lebanese_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (62, 'Mediterranean restaurants', 'mediterranean_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (63, 'Mexican restaurants', 'mexican_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (64, 'Middle eastern restaurants', 'middle_eastern_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (65, 'Pizza restaurants', 'pizza_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (66, 'Ramen restaurants', 'ramen_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (67, 'Restaurants', 'restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (68, 'Sandwich shops', 'sandwich_shop', TRUE, 'SYSTEM', 'SYSTEM'),
    (69, 'Seafood restaurants', 'seafood_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (70, 'Spanish restaurants', 'spanish_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (71, 'Steak houses', 'steak_house', TRUE, 'SYSTEM', 'SYSTEM'),
    (72, 'Sushi restaurants', 'sushi_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (73, 'Thai restaurants', 'thai_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (74, 'Turkish restaurants', 'turkish_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (75, 'Vegan restaurants', 'vegan_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (76, 'Vegetarian restaurants', 'vegetarian_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (77, 'Vietnamese restaurants', 'vietnamese_restaurant', TRUE, 'SYSTEM', 'SYSTEM'),
    (78, 'Chiropractors', 'chiropractor', TRUE, 'SYSTEM', 'SYSTEM'),
    (79, 'Dental clinics', 'dental_clinic', TRUE, 'SYSTEM', 'SYSTEM'),
    (80, 'Dentists', 'dentist', TRUE, 'SYSTEM', 'SYSTEM'),  
    (81, 'Drugstores', 'drugstore', TRUE, 'SYSTEM', 'SYSTEM'),
    (82, 'Massage locations', 'massage', TRUE, 'SYSTEM', 'SYSTEM'),
    (83, 'Physiotherapists', 'physiotherapist', TRUE, 'SYSTEM', 'SYSTEM'),
    (84, 'Saunas', 'sauna', TRUE, 'SYSTEM', 'SYSTEM'),
    (85, 'Skin care clinics', 'skin_care_clinic', TRUE, 'SYSTEM', 'SYSTEM'),
    (86, 'Spas', 'spa', TRUE, 'SYSTEM', 'SYSTEM'),
    (87, 'Tanning studios', 'tanning_studio', TRUE, 'SYSTEM', 'SYSTEM'),
    (88, 'Wellness centers', 'wellness_center', TRUE, 'SYSTEM', 'SYSTEM'),
    (89, 'Yoga studios', 'yoga_studio', TRUE, 'SYSTEM', 'SYSTEM'),
    (90, 'Bed and breakfast locations', 'bed_and_breakfast', TRUE, 'SYSTEM', 'SYSTEM'),
    (91, 'Campgrounds', 'campground', TRUE, 'SYSTEM', 'SYSTEM'),
    (92, 'Cottages', 'cottage', TRUE, 'SYSTEM', 'SYSTEM'),
    (93, 'Hotels', 'hotel', TRUE, 'SYSTEM', 'SYSTEM'),
    (94, 'Inns', 'inn', TRUE, 'SYSTEM', 'SYSTEM'),
    (95, 'Motels', 'motel', TRUE, 'SYSTEM', 'SYSTEM'),
    (96, 'Resort hotels', 'resort_hotel', TRUE, 'SYSTEM', 'SYSTEM'),
    (97, 'RV parks', 'rv_park', TRUE, 'SYSTEM', 'SYSTEM'),
    (98, 'Barber shops', 'barber_shop', TRUE, 'SYSTEM', 'SYSTEM'),
    (99, 'Beauticians', 'beautician', TRUE, 'SYSTEM', 'SYSTEM'),
    (100, 'Beauty salons', 'beauty_salon', TRUE, 'SYSTEM', 'SYSTEM'),
    (101, 'Catering services', 'catering_service', TRUE, 'SYSTEM', 'SYSTEM'),
    (102, 'Florists', 'florist', TRUE, 'SYSTEM', 'SYSTEM'),
    (103, 'Food delivery services', 'food_delivery', TRUE, 'SYSTEM', 'SYSTEM'),
    (104, 'Foot care locations', 'foot_care', TRUE, 'SYSTEM', 'SYSTEM'),
    (105, 'Hair care locations', 'hair_care', TRUE, 'SYSTEM', 'SYSTEM'),
    (106, 'Hair salons', 'hair_salon', TRUE, 'SYSTEM', 'SYSTEM'),
    (107, 'Laundry services', 'laundry', TRUE, 'SYSTEM', 'SYSTEM'),
    (108, 'Makeup artists', 'makeup_artist', TRUE, 'SYSTEM', 'SYSTEM'),
    (109, 'Moving companies', 'moving_company', TRUE, 'SYSTEM', 'SYSTEM'),
    (110, 'Nail salons', 'nail_salon', TRUE, 'SYSTEM', 'SYSTEM'),
    (111, 'Psychics', 'psychic', TRUE, 'SYSTEM', 'SYSTEM'),
    (112, 'Storage locations', 'storage', TRUE, 'SYSTEM', 'SYSTEM'),
    (113, 'Tailors', 'tailor', TRUE, 'SYSTEM', 'SYSTEM'),
    (114, 'Veterinarians', 'veterinary_care', TRUE, 'SYSTEM', 'SYSTEM'),
    (115, 'Asian grocery stores', 'asian_grocery_store', TRUE, 'SYSTEM', 'SYSTEM'),
    (116, 'Auto parts stores', 'auto_parts_store', TRUE, 'SYSTEM', 'SYSTEM'),
    (117, 'Bicycle stores', 'bicycle_store', TRUE, 'SYSTEM', 'SYSTEM'),
    (118, 'Book stores', 'book_store', TRUE, 'SYSTEM', 'SYSTEM'),
    (119, 'Butcher shops', 'butcher_shop', TRUE, 'SYSTEM', 'SYSTEM'),
    (120, 'Cell phone stores', 'cell_phone_store', TRUE, 'SYSTEM', 'SYSTEM'),
    (121, 'Clothing stores', 'clothing_store', TRUE, 'SYSTEM', 'SYSTEM'),
    (122, 'Convenience stores', 'convenience_store', TRUE, 'SYSTEM', 'SYSTEM'),
    (123, 'Department stores', 'department_store', TRUE, 'SYSTEM', 'SYSTEM'),
    (124, 'Discount stores', 'discount_store', TRUE, 'SYSTEM', 'SYSTEM'),
    (125, 'Appliance stores', 'electronics_store', TRUE, 'SYSTEM', 'SYSTEM'),
    (126, 'Food stores', 'food_store', TRUE, 'SYSTEM', 'SYSTEM'),
    (127, 'Furniture stores', 'furniture_store', TRUE, 'SYSTEM', 'SYSTEM'),
    (128, 'Gift shops', 'gift_shop', TRUE, 'SYSTEM', 'SYSTEM'),
    (129, 'Grocery stores', 'grocery_store', TRUE, 'SYSTEM', 'SYSTEM'),
    (130, 'Hardware stores', 'hardware_store', TRUE, 'SYSTEM', 'SYSTEM'),
    (131, 'Home goods stores', 'home_goods_store', TRUE, 'SYSTEM', 'SYSTEM'),
    (132, 'Home improvement stores', 'home_improvement_store', TRUE, 'SYSTEM', 'SYSTEM'),
    (133, 'Jewelry stores', 'jewelry_store', TRUE, 'SYSTEM', 'SYSTEM'),
    (134, 'Markets', 'market', TRUE, 'SYSTEM', 'SYSTEM'),
    (135, 'Pet stores', 'pet_store', TRUE, 'SYSTEM', 'SYSTEM'),
    (136, 'Shoe stores', 'shoe_store', TRUE, 'SYSTEM', 'SYSTEM'),
    (137, 'Shopping malls', 'shopping_mall', TRUE, 'SYSTEM', 'SYSTEM'),
    (138, 'Supermarkets', 'supermarket', TRUE, 'SYSTEM', 'SYSTEM'),
    (139, 'Fishing charters', 'fishing_charter', TRUE, 'SYSTEM', 'SYSTEM'),
    (140, 'Ice skating rinks', 'ice_skating_rink', TRUE, 'SYSTEM', 'SYSTEM'),
    (141, 'Ski resorts', 'ski_resort', TRUE, 'SYSTEM', 'SYSTEM'),
    (142, 'Stadiums', 'stadium', TRUE, 'SYSTEM', 'SYSTEM');

-- Insert values into wallet_category table
DO $$
DECLARE
    v_tenant_code       text   := '<HAP-TENANT-CODE>';
    v_wallet_type_id    int;
    v_wallet_type_code  text;
    v_create_user       text   := 'SYSTEM';
    v_update_user       text   := 'SYSTEM';
BEGIN
    -- Get wallet_type_id and wallet_type_code dynamically
    SELECT wt.wallet_type_id, wt.wallet_type_code
    INTO v_wallet_type_id, v_wallet_type_code
    FROM (
        SELECT
            (jsonb_path_query(
                tenant_config_json,
                '$.purseConfig.purses[*] ? (@.purseLabel == "HLTHLVNG").purseWalletType'
            ) #>> '{}') AS purse_wallet_type
        FROM fis.tenant_account
        WHERE tenant_code = v_tenant_code
    ) pe
    JOIN wallet.wallet_type wt
      ON wt.wallet_type_code = pe.purse_wallet_type;

    -- Insert into wallet_category
    WITH last_id AS (
	    SELECT COALESCE(MAX(id), 0) AS max_id
	    FROM tenant.wallet_category
	),
	numbered AS (
	    SELECT 
	        row_number() OVER () + (SELECT max_id FROM last_id) AS new_id,
	        c.*
	    FROM tenant.category c
	)
	INSERT INTO tenant.wallet_category
	    (id, tenant_code, wallet_type_id, wallet_type_code, category_fk,
	     config_json, create_user, update_user, delete_nbr)
	SELECT
	    new_id,
	    v_tenant_code,
	    v_wallet_type_id,
	    v_wallet_type_code,
	    c.id,
	    jsonb_build_object('api_source', 'GOOGLE', 'google_type', c.google_type, 'display_name', c.name)::jsonb,
	    v_create_user,
	    v_update_user,
	    0
	FROM numbered c;
END $$;

DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>';
BEGIN
    UPDATE tenant.wallet_category wc
    SET config_json = jsonb_set(
        config_json,
        '{initial_priority}',
        to_jsonb(CASE (config_json ->> 'display_name')
            WHEN 'Movie theaters'       THEN 4
            WHEN 'Hair salons'          THEN 2
            WHEN 'Department stores'    THEN 1
            WHEN 'Gift shops'           THEN 3
            ELSE NULL
        END),
        true
    )
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0
      AND (config_json ->> 'display_name') IN (
          'Movie theaters',
          'Hair salons',
          'Department stores',
          'Gift shops'
      );

    RAISE NOTICE 'Updated config_json with initial_priority for tenant %', v_tenant_code;
END $$;

DO $$
DECLARE
    v_tenant_code       text   := '<KP-TENANT-CODE>';
    v_wallet_type_id    int;
    v_wallet_type_code  text;
    v_create_user       text   := 'SYSTEM';
    v_update_user       text   := 'SYSTEM';
BEGIN
    -- Get wallet_type_id and wallet_type_code dynamically
    SELECT wt.wallet_type_id, wt.wallet_type_code
    INTO v_wallet_type_id, v_wallet_type_code
    FROM (
        SELECT
            (jsonb_path_query(
                tenant_config_json,
                '$.purseConfig.purses[*] ? (@.purseLabel == "HLTHLVNG").purseWalletType'
            ) #>> '{}') AS purse_wallet_type
        FROM fis.tenant_account
        WHERE tenant_code = v_tenant_code
    ) pe
    JOIN wallet.wallet_type wt
      ON wt.wallet_type_code = pe.purse_wallet_type;

    -- Insert into wallet_category
    WITH last_id AS (
	    SELECT COALESCE(MAX(id), 0) AS max_id
	    FROM tenant.wallet_category
	),
	numbered AS (
	    SELECT 
	        row_number() OVER () + (SELECT max_id FROM last_id) AS new_id,
	        c.*
	    FROM tenant.category c
	)
	INSERT INTO tenant.wallet_category
	    (id, tenant_code, wallet_type_id, wallet_type_code, category_fk,
	     config_json, create_user, update_user, delete_nbr)
	SELECT
	    new_id,
	    v_tenant_code,
	    v_wallet_type_id,
	    v_wallet_type_code,
	    c.id,
	    jsonb_build_object('api_source', 'GOOGLE', 'google_type', c.google_type, 'display_name', c.name)::jsonb,
	    v_create_user,
	    v_update_user,
	    0
	FROM numbered c;
END $$;

DO $$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>';
BEGIN
    UPDATE tenant.wallet_category wc
    SET config_json = jsonb_set(
        config_json,
        '{initial_priority}',
        to_jsonb(CASE (config_json ->> 'display_name')
            WHEN 'Movie theaters'       THEN 4
            WHEN 'Hair salons'          THEN 2
            WHEN 'Department stores'    THEN 1
            WHEN 'Gift shops'           THEN 3
            ELSE NULL
        END),
        true
    )
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0
      AND (config_json ->> 'display_name') IN (
          'Movie theaters',
          'Hair salons',
          'Department stores',
          'Gift shops'
      );

    RAISE NOTICE 'Updated config_json with initial_priority for tenant %', v_tenant_code;
END $$;

DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>'; 
BEGIN
    -- Update ONLY FIS Network categories
    UPDATE tenant.wallet_category wc
    SET config_json = jsonb_set(
        config_json,
        '{api_source}',
        to_jsonb('FIS'::text),
        true
    )
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0
      AND (config_json ->> 'display_name') IN (
          'Drugstores',
          'Grocery stores',
          'Supermarkets'
      );
    RAISE NOTICE 'Updated FIS categories for tenant %', v_tenant_code;
END $$;