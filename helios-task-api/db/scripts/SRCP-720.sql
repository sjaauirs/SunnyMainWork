ALTER TABLE task.task_reward
ADD column IF NOT EXISTS task_external_code varchar(80) not null default 'NA';
 
CREATE UNIQUE INDEX IF NOT EXISTS idx_task_reward_2
    ON task.task_reward (task_external_code, tenant_code, delete_nbr);

  
-- create a function
CREATE OR REPLACE FUNCTION task.generate_identifier(input_text text)
RETURNS text AS $$
DECLARE
    cleaned_string text;
    words text[];
    selected_words text[];
    identifier text;
BEGIN
    -- Remove punctuation from the input string
    cleaned_string := regexp_replace(input_text, E'[^\\w\\s]', '', 'g');

    -- Split the cleaned string into words
    words := regexp_split_to_array(trim(cleaned_string), E'\\s+');

    -- Take the first 10 words (or less if there are fewer than 10 words)
    selected_words := words[1:LEAST(array_length(words, 1), 10)];

    -- Ensure that the selected words start with lowercase alphabets
    selected_words := array(select lower(word) from unnest(selected_words) as word);
     
    -- Take the first 4 characters from each word and concatenate with "_" separator
    identifier := array_to_string(array_agg(left(word, 4)), '_') FROM (SELECT unnest(selected_words) AS word) AS subquery;

    RETURN identifier;
END;
$$ LANGUAGE plpgsql;


-- exec generate_identifier function
DO $$
DECLARE 
    row_data text;
    identifier_result text;
    tdtr_cursor CURSOR FOR 
        SELECT td.task_header, tr.task_external_code
        FROM task.task_detail td
        INNER JOIN task.task_reward tr ON td.task_id = tr.task_id AND td.tenant_code = tr.tenant_code
        ORDER BY tr.task_reward_id ASC
        FOR UPDATE;
BEGIN
    OPEN tdtr_cursor;
    LOOP
        FETCH tdtr_cursor INTO row_data, identifier_result;
        EXIT WHEN NOT FOUND;

        -- Assign the result of the SELECT to the variable
        identifier_result := task.generate_identifier(row_data);
        
        -- Update the current row with the generated identifier
        UPDATE task.task_reward
        SET task_external_code = identifier_result
        WHERE CURRENT OF tdtr_cursor;
        
        -- Now you can use the identifier_result variable as needed
        RAISE NOTICE 'Identifier: %', identifier_result;
    END LOOP;
    CLOSE tdtr_cursor;
END $$;
