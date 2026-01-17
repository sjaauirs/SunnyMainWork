
INSERT INTO task.trivia_question(
	trivia_question_code, trivia_json, create_ts, update_ts, create_user, update_user, delete_nbr, question_external_code)
  VALUES (
	'trq-' || REPLACE(CAST(gen_random_uuid() as varchar), '-', ''), '{
  "answerType": "SINGLE",
  "layoutType": "BUTTON",
  "questionText": "What does your credit score impact?",
  "answerText": [
    "Loan terms",
    "Your marriage",
    "Golf handicap"
  ],
  "correctAnswer": [
    0
  ]
}', now() at time zone 'utc', null, 'per-915325069cdb42c783dd4601e1d27704', null, 0, 'what_does_your_cred_scor_impa');
	
INSERT INTO task.trivia_question(
	trivia_question_code, trivia_json, create_ts, update_ts, create_user, update_user, delete_nbr, question_external_code)
  VALUES (
	'trq-' || REPLACE(CAST(gen_random_uuid() as varchar), '-', ''), '{
  "answerType": "SINGLE",
  "layoutType": "BUTTON",
  "questionText": "How many months of living expenses do you need in an emergeny fund?",
  "answerText": [
    "1-3 months",
    "3-6 months",
    "6-9 months"
  ],
  "correctAnswer": [
    1
  ]
}
', now() at time zone 'utc', null, 'per-915325069cdb42c783dd4601e1d27704', null, 0, 'how_many_mont_of_livi_expe_do_you_need_in');  
  
INSERT INTO task.trivia_question(
	trivia_question_code, trivia_json, create_ts, update_ts, create_user, update_user, delete_nbr, question_external_code)
  VALUES (
	'trq-' || REPLACE(CAST(gen_random_uuid() as varchar), '-', ''), '{
  "answerType": "SINGLE",
  "layoutType": "BUTTON",
  "questionText": "Is Pluto a planet?",
  "answerText": [
    "Yes",
    "No"
  ],
  "correctAnswer": [
    1
  ]
}', now() at time zone 'utc', null, 'per-915325069cdb42c783dd4601e1d27704', null, 0, 'is_plut_a_plan');  
  

INSERT INTO task.trivia(
	trivia_code, task_reward_id, cta_task_external_code, config_json, create_ts, update_ts, create_user, update_user, delete_nbr)
	VALUES ('trv-' || REPLACE(CAST(gen_random_uuid() as varchar), '-', ''), (select task_reward_id from task.task_reward where task_external_code = 'play_mont_triv'), null, '{
"ux": {
	"backgroundUrl": "",
	"questionIcon": {
    "url":  "",
    "fgColor": "#FFFFFF",
    "bgColor": "#111111"
},

	"correctAnswerIcon": {
    "url":  "",
    "fgColor": "#FFFFFF",
    "bgColor": "#111111"
},
	"wrongAnswerIcon": {
    "url":  "",
    "fgColor": "#FFFFFF",
    "bgColor": "#FF0000"
}
}
}', now() at time zone 'utc', null, 'per-915325069cdb42c783dd4601e1d27704', null, 0);  

update task.task set task_type_id = 2 where task_id = (select task_id from task.task_reward where task_external_code = 'play_mont_triv');

INSERT INTO task.trivia_question_group(
	trivia_id, trivia_question_id, sequence_nbr, create_ts, update_ts, create_user, update_user, delete_nbr)
	VALUES ((select trivia_id from task.trivia where task_reward_id=(select task_reward_id from task.task_reward where task_external_code = 'play_mont_triv')), (select trivia_question_id from task.trivia_question where question_external_code='what_does_your_cred_scor_impa'), 0, now() at time zone 'utc', null, 'per-915325069cdb42c783dd4601e1d27704', null, 0);
INSERT INTO task.trivia_question_group(
	trivia_id, trivia_question_id, sequence_nbr, create_ts, update_ts, create_user, update_user, delete_nbr)
	VALUES ((select trivia_id from task.trivia where task_reward_id=(select task_reward_id from task.task_reward where task_external_code = 'play_mont_triv')), (select trivia_question_id from task.trivia_question where question_external_code='how_many_mont_of_livi_expe_do_you_need_in'), 1, now() at time zone 'utc', null, 'per-915325069cdb42c783dd4601e1d27704', null, 0);
INSERT INTO task.trivia_question_group(
	trivia_id, trivia_question_id, sequence_nbr, create_ts, update_ts, create_user, update_user, delete_nbr)
	VALUES ((select trivia_id from task.trivia where task_reward_id=(select task_reward_id from task.task_reward where task_external_code = 'play_mont_triv')), (select trivia_question_id from task.trivia_question where question_external_code='is_plut_a_plan'), 2, now() at time zone 'utc', null, 'per-915325069cdb42c783dd4601e1d27704', null, 0);