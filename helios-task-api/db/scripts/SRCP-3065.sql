-- Add the columns with the default values for new rows
ALTER TABLE task.trivia_question_group 
ADD COLUMN IF NOT EXISTS valid_start_ts timestamp DEFAULT (now() AT TIME ZONE 'UTC' - INTERVAL '1 month'),
ADD COLUMN IF NOT EXISTS valid_end_ts timestamp DEFAULT (now() AT TIME ZONE 'UTC' + INTERVAL '1 month');

