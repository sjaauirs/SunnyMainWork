-- Rollback trivia_question  for multiple json in trivia_json


UPDATE task.trivia_question
SET trivia_json = trivia_json -> 'en-US'
WHERE trivia_json ? 'en-US';