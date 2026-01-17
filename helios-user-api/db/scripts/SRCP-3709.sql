-- Script to add a new column agreement_status if it doesn't already exist

ALTER TABLE huser.consumer
ADD COLUMN IF NOT EXISTS agreement_status VARCHAR(80) NOT NULL DEFAULT 'NOT_VERIFIED';


--update agreement_status to 'VERIFIED' for existing consumers where onboarding_state = 'VERIFIED'
update huser.consumer set agreement_status =onboarding_state,
update_ts=CURRENT_TIMESTAMP AT TIME ZONE 'UTC'
where onboarding_state='VERIFIED' and agreement_status <>'VERIFIED'
