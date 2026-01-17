DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'huser' 
          AND table_name = 'person' 
          AND column_name = 'age'
    ) THEN
        ALTER TABLE huser.person
        DROP COLUMN age;
    END IF;
END
$$;
