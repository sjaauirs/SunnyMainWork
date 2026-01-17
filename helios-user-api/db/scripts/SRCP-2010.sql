ALTER table huser.person
ADD COLUMN IF NOT EXISTS ssn varchar(255) not null default 'test';

ALTER table huser.person
ADD COLUMN IF NOT EXISTS ssn_last4 varchar(4) not null default '0000';

ALTER table huser.person
ADD COLUMN IF NOT EXISTS mailing_addr_line_1 varchar(255) not null default 'test';

ALTER table huser.person
ADD COLUMN IF NOT EXISTS mailing_addr_line_2 varchar(80) not null default 'test';

ALTER table huser.person
ADD COLUMN IF NOT EXISTS mailing_state varchar(80) not null default 'test';

ALTER table huser.person
ADD COLUMN IF NOT EXISTS mailing_country_code varchar(10) not null default '+1';

ALTER table huser.person
ADD COLUMN IF NOT EXISTS home_phone_number varchar(20) not null default '0000000000';