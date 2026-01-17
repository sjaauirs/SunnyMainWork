ALTER TABLE huser.person
    ALTER COLUMN "mailing_addr_line_1" SET NOT NULL,
    ALTER COLUMN "mailing_addr_line_2" SET NOT NULL,
    ALTER COLUMN "mailing_state" SET NOT NULL,
    ALTER COLUMN "mailing_country_code" SET NOT NULL,
    ALTER COLUMN "home_phone_number" SET NOT NULL,
    ALTER COLUMN "city" SET NOT NULL,
    ALTER COLUMN "country" SET NOT NULL,
    ALTER COLUMN "postal_code" SET NOT NULL;

