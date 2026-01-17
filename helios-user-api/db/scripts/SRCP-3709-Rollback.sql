-- Script to remove the agreement_status column if it exists

ALTER TABLE huser.consumer
DROP COLUMN IF EXISTS agreement_status;