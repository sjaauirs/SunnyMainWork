-- Delete address_type_name 'MAILING' from address_type table
DO $$
BEGIN
    DELETE FROM huser.address_type
    WHERE LOWER(address_type_name) = 'mailing'
      AND delete_nbr = 0
      AND create_user = 'system';
END $$;

-- Drop indexes
DROP INDEX IF EXISTS huser.one_primary_address_per_person;
DROP INDEX IF EXISTS huser.idx_address_person_id;
DROP INDEX IF EXISTS huser.idx_address_person_primary;

-- Drop table: person_address
DROP TABLE IF EXISTS huser.person_address;

-- Drop table: address_type
DROP TABLE IF EXISTS huser.address_type;