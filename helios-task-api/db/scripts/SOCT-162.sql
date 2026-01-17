--update all existing trivia_questions trivia_json for languageCode
--  ensures idempotency update only when trivia_json doesnot have key en-us/en

UPDATE task.trivia_question
SET trivia_json = jsonb_build_object(
    'en-US', trivia_json
)
WHERE NOT (
    jsonb_typeof(trivia_json) = 'object'
    AND trivia_json ? 'en-US'
);
