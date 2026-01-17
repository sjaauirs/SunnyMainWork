DO $$
BEGIN
    -- Add 'age' column if it doesn't exist

    IF NOT EXISTS (

        SELECT 1 FROM information_schema.columns 

        WHERE table_schema = 'huser' 

          AND table_name = 'person' 

          AND column_name = 'age'

    ) THEN

        ALTER TABLE huser.person

        ADD COLUMN age INT NOT NULL DEFAULT 0;

    END IF;
 
    -- Safely update age only for realistic DOBs

    UPDATE huser.person

    SET age = DATE_PART('year', AGE(CURRENT_DATE, dob))

    WHERE dob IS NOT NULL

      AND dob <= CURRENT_DATE

      AND dob >= DATE '1900-01-01';

END

$$;

 
