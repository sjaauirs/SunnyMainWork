-- Use below script to create secondary wallets for existing owner type consumers
-- We have a separate script to create secondary wallets for existing dependent type consumers
-- We have to update only tenant_code_value in this file


DO $$
DECLARE
	tenant_code_value varchar(50) := 'ten-87cfa20e9d7140ec9294ae3342d79db0';
	primary_wallet_type_code varchar(50) := 'wat-2d62dcaf2aa4424b9ff6c2ddb5895077';
	secondary_wallet_type_code  varchar(50) := 'wat-c3b091232e974f98aeceb495d2a9f916';
    consumer_role_value char(1) := 'O';
    wallet_name_value varchar(80) := 'SWEEPSTAKES_REWARD';
    primary_wallet_type_id int;
    secondary_wallet_type_id int;
    secondary_wallet_exists boolean;
    wallet_customer_code_value varchar(50);
    wallet_code_value varchar(50);
    current_year int;
    active_end_ts_value timestamp;
	earn_maximum_value NUMERIC(14, 2);
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
			--LIMIT 4 OFFSET 2;
BEGIN 

	 -- Fetch Primary wallet type id
    SELECT wallet_type_id INTO primary_wallet_type_id
    FROM wallet.wallet_type
    WHERE wallet_type_code = primary_wallet_type_code;
	
	 -- Fetch Secondary wallet type id
    SELECT wallet_type_id INTO secondary_wallet_type_id
    FROM wallet.wallet_type
    WHERE wallet_type_code = secondary_wallet_type_code;
	RAISE NOTICE 'secondary_wallet_type_id wallet id %', secondary_wallet_type_id;
	
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
			If Not secondary_wallet_exists THEN
				
				-- Genarate wallet code
				select 'wal-' || REPLACE(CAST(gen_random_uuid() as varchar), '-', '') INTO wallet_code_value;
				-- Genarate wallet customer code
				select 'cus-' || REPLACE(CAST(gen_random_uuid() as varchar), '-', '') INTO wallet_customer_code_value;
				-- Generate active_end_ts
				SELECT EXTRACT(YEAR FROM CURRENT_TIMESTAMP) INTO current_year;
				SELECT MAKE_TIMESTAMP(current_year, 12, 31, 23, 59, 59) AT TIME ZONE 'UTC' INTO active_end_ts_value;
				
				--- Get earn_maximum of primary wallet
				 SELECT w.earn_maximum INTO earn_maximum_value FROM wallet.wallet w
					JOIN wallet.consumer_wallet cwt ON w.wallet_id = cwt.wallet_id
					AND w.tenant_code = cwt.tenant_code
					WHERE w.wallet_type_id = primary_wallet_type_id
					AND cwt.consumer_code = consumer_record.consumer_code
					AND cwt.tenant_code = tenant_code_value
					AND cwt.consumer_role = consumer_role_value;
				
				-- Create secondary wallet
				RAISE NOTICE '---- started creating secondary wallet for consumer_code: %  -----', consumer_record.consumer_code; 
				INSERT INTO wallet.wallet (wallet_type_id, customer_code, sponsor_code, tenant_code, wallet_code, wallet_name, active_start_ts, active_end_ts, balance, earn_maximum, create_ts, update_ts, create_user, update_user, delete_nbr, total_earned, master_wallet, active)
				VALUES (
					secondary_wallet_type_id,
					wallet_customer_code_value,
					NULL,
					tenant_code_value,
					wallet_code_value,
					wallet_name_value,
					NOW(), --  active_start_ts
					active_end_ts_value, -- active_end_ts
					0, -- Initial balance
					earn_maximum_value, -- earn_maximum
					NOW(), -- create_ts
					NULL, -- update_ts
					'SYSTEM', -- create_user
					NULL, -- update_user
					0, -- delete_nbr
					0, -- total_earned
					false, -- master_wallet
					true -- active
				) RETURNING wallet_id INTO consumer_record.wallet_id;

				RAISE NOTICE 'created wallet for consumer_code:%  wallet_id: %', consumer_record.consumer_code, consumer_record.wallet_id; 
				-- Create consumer wallet
				INSERT INTO wallet.consumer_wallet (wallet_id, tenant_code, consumer_code, consumer_role, earn_maximum, create_ts, update_ts, create_user, update_user, delete_nbr)
				VALUES (
					consumer_record.wallet_id,
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
				RAISE NOTICE 'created consumer wallet for consumer_code:%  wallet_id: %', consumer_record.consumer_code, consumer_record.wallet_id; 
				RAISE NOTICE '---- successfully ceated secondary wallet for consumer_code: %  -----', consumer_record.consumer_code; 
			ELSE
				RAISE NOTICE '---- Secondary wallet already exist, consumer_code: %  -----', consumer_record.consumer_code; 
			END IF;
        END;
	END LOOP;

    -- Close the cursor
    CLOSE cursor_consumer;
END $$;