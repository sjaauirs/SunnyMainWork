ALTER TABLE huser.person ADD COLUMN IF NOT EXISTS synthetic_user BOOLEAN not null default false;
