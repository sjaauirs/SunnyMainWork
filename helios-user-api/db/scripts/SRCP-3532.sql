ALTER table huser.consumer
ADD COLUMN IF NOT EXISTS onboarding_state varchar(50) not null default 'NOT_STARTED';



UPDATE huser.consumer c
SET onboarding_state = p.onboarding_state
FROM huser.person p
WHERE c.person_id = p.person_id;

