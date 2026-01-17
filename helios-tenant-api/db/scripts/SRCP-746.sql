ALTER TABLE tenant.tenant
ADD COLUMN redemption_vendor_name_0  varchar(255) not null default 'PRIZEOUT';

ALTER TABLE tenant.tenant
ADD COLUMN redemption_vendor_partner_id_0 varchar(255) not null default '11ed00f7-10fb-4f45-b00c-d95a929cab53';