ALTER table huser.person
ADD COLUMN IF NOT EXISTS is_spouse BOOLEAN not null default false;

ALTER table huser.person
ADD COLUMN IF NOT EXISTS is_dependent BOOLEAN not null default false;


