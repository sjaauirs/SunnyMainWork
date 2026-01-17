--Use below script to return the list of consumer_codes and emailIDs of consumers who have non-zero entries (secondary wallet balance > 0)
-- for a given tenant
select cmr.consumer_code, per.email
	from huser.person per
	join huser.consumer cmr on per.person_id = cmr.person_id
	join wallet.consumer_wallet cwt on cmr.consumer_code = cwt.consumer_code and cmr.tenant_code = cwt.tenant_code
	join wallet.wallet wt on cwt.wallet_id = wt.wallet_id
where 
	wt.wallet_type_id in (SELECT wallet_type_id 
		FROM wallet.wallet_type
		WHERE wallet_type_code = 'wat-c3b091232e974f98aeceb495d2a9f916') -- get secondary wallet type id
	and wt.balance > 0
	and cmr.tenant_code = 'ten-ecada21e57154928a2bb959e8365b8b4' -- filter by tenant code
	and cwt.consumer_role = 'O' -- filter by consumer as owner
	and cwt.delete_nbr = 0
	and cmr.delete_nbr = 0
	and wt.delete_nbr = 0
