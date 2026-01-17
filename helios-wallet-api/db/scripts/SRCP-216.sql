-- reverse reln between wallet.transaction and wallet.transaction_detail

-- backup tables
CREATE TABLE wallet.transaction_copy AS table wallet.transaction;
CREATE TABLE wallet.transaction_detail_copy AS table wallet.transaction_detail;

-- drop FK from transaction_detail table
ALTER TABLE wallet.transaction_detail DROP CONSTRAINT fk_transaction_id;

-- add new column into transaction table
ALTER TABLE wallet.transaction
ADD COLUMN transaction_detail_id  bigint not null default 0;

-- populate thru join
UPDATE wallet.transaction AS t
SET t.transaction_detail_id = td.transaction_detail_id
FROM wallet.transaction_detail AS td
WHERE t.transaction_id = td.transaction_id

-- enforce constraint
ALTER TABLE wallet.transaction
Add constraint fk_transaction_detail_id
FOREIGN KEY(transaction_detail_id) 
REFERENCES wallet.transaction_detail(transaction_detail_id);

-- drop from other
ALTER TABLE wallet.transaction_detail
DROP COLUMN transaction_id;

-- DROP TABLE wallet.transaction_copy;
-- DROP TABLE wallet.transaction_detail_copy;