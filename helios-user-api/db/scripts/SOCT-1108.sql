ALTER TABLE huser.person
    ALTER COLUMN "mailing_addr_line_1" DROP NOT NULL,
    ALTER COLUMN "mailing_addr_line_2" DROP NOT NULL,
    ALTER COLUMN "mailing_state" DROP NOT NULL,
    ALTER COLUMN "mailing_country_code" DROP NOT NULL,
    ALTER COLUMN "home_phone_number" DROP NOT NULL,
    ALTER COLUMN "city" DROP NOT NULL,
    ALTER COLUMN "country" DROP NOT NULL,
    ALTER COLUMN "postal_code" DROP NOT NULL;
