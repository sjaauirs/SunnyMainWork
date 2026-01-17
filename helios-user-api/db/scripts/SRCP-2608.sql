ALTER table huser.person
ADD COLUMN IF NOT EXISTS onboarding_state varchar(20) not null default 'NOT_STARTED';