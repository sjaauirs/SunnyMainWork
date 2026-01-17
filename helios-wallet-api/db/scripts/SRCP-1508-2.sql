
-- Before executing this script we need to execute creat secondary wallet for existing owner type consumers script, file Name- SRCP-1508-1.sql
-- Use below script to create secondary wallets for existing dependent type consumers
-- We have to update only tenant_code_value in this file

DO $$
DECLARE
	tenant_code_value varchar(50) := 'ten-87cfa20e9d7140ec9294ae3342d79db0';
	primary_wallet_type_code varchar(50) := 'wat-2d62dcaf2aa4424b9ff6c2ddb5895077';
	secondary_wallet_type_code  varchar(50) := 'wat-c3b091232e974f98aeceb495d2a9f916';
    consumer_role_value char(1) := 'C';
    primary_wallet_type_id int;
    secondary_wallet_type_id int;
    secondary_wallet_exists boolean;
	consumer_wallet_id_value int;
	consumer_code_of_owner varchar(50);
	secondary_wallet_id_of_owner int;
    consumer_record RECORD;
    cursor_consumer CURSOR FOR 
	   SELECT cw.consumer_wallet_id, cw.wallet_id, cw.tenant_code, cw.consumer_code, cw.consumer_role, cw.delete_nbr
			FROM wallet.consumer_wallet cw
			JOIN wallet.wallet w ON cw.wallet_id = w.wallet_id
			WHERE cw.delete_nbr = 0
			AND w.wallet_type_id = primary_wallet_type_id
			AND cw.consumer_role = consumer_role_value
			AND cw.tenant_code = tenant_code_value 
			AND w.master_wallet = false;
			--LIMIT 2 OFFSET 0;
BEGIN 

	 -- Fetch Primary wallet type id
    SELECT wallet_type_id INTO primary_wallet_type_id
    FROM wallet.wallet_type
    WHERE wallet_type_code = primary_wallet_type_code;
	
	 -- Fetch Secondary wallet type id
    SELECT wallet_type_id INTO secondary_wallet_type_id
    FROM wallet.wallet_type
    WHERE wallet_type_code = secondary_wallet_type_code;
	
	
	-- Open the cursor
	OPEN cursor_consumer;
	
	-- Loop through each consumer wallet record
	LOOP
		FETCH cursor_consumer INTO consumer_record;
		
		-- Exit loop if no more records
		EXIT WHEN NOT FOUND;
		
		-- Check for secondary wallet existence
		BEGIN
			SELECT INTO secondary_wallet_exists EXISTS (
				SELECT 1 FROM wallet.wallet w
				JOIN wallet.consumer_wallet cwt ON w.wallet_id = cwt.wallet_id
				AND w.tenant_code = cwt.tenant_code
				WHERE w.wallet_type_id = secondary_wallet_type_id
				AND cwt.consumer_code = consumer_record.consumer_code
				AND cwt.tenant_code = tenant_code_value
				AND cwt.consumer_role = consumer_role
			);
			
			-- If secondary wallet does not exist, create it
			IF Not secondary_wallet_exists THEN
				
				-- Get primary wallet id for consumer
                SELECT w.wallet_id INTO consumer_wallet_id_value FROM wallet.wallet w
					JOIN wallet.consumer_wallet cwt ON w.wallet_id = cwt.wallet_id
					AND w.tenant_code = cwt.tenant_code
					WHERE w.wallet_type_id = primary_wallet_type_id
					AND cwt.consumer_code = consumer_record.consumer_code
					AND cwt.tenant_code = tenant_code_value;
					
				-- Get consumer_code_of_owner
				SELECT consumer_code INTO consumer_code_of_owner
					FROM wallet.consumer_wallet
					WHERE wallet_id = consumer_wallet_id_value and consumer_role = 'O';
					
				-- Get secondary wallet id of owner
			     SELECT w.wallet_id INTO secondary_wallet_id_of_owner FROM wallet.wallet w
					JOIN wallet.consumer_wallet cwt ON w.wallet_id = cwt.wallet_id
					AND w.tenant_code = cwt.tenant_code
					WHERE w.wallet_type_id = secondary_wallet_type_id
					AND cwt.consumer_code = consumer_code_of_owner
					AND cwt.tenant_code = tenant_code_value
					AND cwt.consumer_role = 'O';
					
					
				IF secondary_wallet_id_of_owner IS NOT NULL THEN
					RAISE NOTICE '---- started creating secondary wallet for consumer_code: %  -----', consumer_record.consumer_code; 
					-- Create consumer wallet
					INSERT INTO wallet.consumer_wallet (wallet_id, tenant_code, consumer_code, consumer_role, earn_maximum, create_ts, update_ts, create_user, update_user, delete_nbr)
					VALUES (
						secondary_wallet_id_of_owner,
						tenant_code_value,
						consumer_record.consumer_code,
						consumer_role_value,
						0.0, -- earn_maximum
						NOW(), -- create_ts
						NULL, -- update_ts
						'SYSTEM', -- create_user
						NULL, -- update_user
						0 -- delete_nbr
					);
					RAISE NOTICE '---- successfully ceated secondary wallet for consumer_code: %  -----', consumer_record.consumer_code; 
				ELSE
					RAISE NOTICE 'secondary_wallet_id_of_owner not exist consumer: %', consumer_record.consumer_code;
				END IF;
			ELSE
				RAISE NOTICE '---- Secondary wallet already exist, consumer_code: %  -----', consumer_record.consumer_code; 
			END IF;
        END;
	END LOOP;

    -- Close the cursor
    CLOSE cursor_consumer;
END $$;