ALTER TABLE task.reward_type ADD COLUMN IF NOT EXISTS reward_type_code varchar(50) not null default 'test';

update task.reward_type set reward_type_code='rtc-a5a943d3fc2a4506ab12218204d60805' where reward_type_name='MONETARY_DOLLARS';
update task.reward_type set reward_type_code='rtc-74a93c5f7ef44020a4314b49936c5955' where reward_type_name='SWEEPSTAKES_ENTRIES';