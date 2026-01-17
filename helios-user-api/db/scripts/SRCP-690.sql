create index idx_task_reward_0 on task.task_reward(reward_type_id);

create index idx_task_reward_1 on task.task_reward(task_id);

create index idx_task_0 on task.task(task_type_id);

create index idx_task_details_0 on task.task_detail(terms_of_service_id);

create index idx_sponsor_0 on tenant.sponsor(customer_id);

create index idx_tenant_0 on tenant.tenant(sponsor_id);

create index idx_consumer_0 on huser.consumer(person_id);

create index idx_redemption_0 on wallet.redemption(revert_add_transaction_id);

create index idx_redemption_1 on wallet.redemption(revert_sub_transaction_id);

create index idx_transaction_0 on wallet.transaction(wallet_id);

create index idx_transaction_1 on wallet.transaction(transaction_detail_id);

create index idx_wallet_0 on wallet.wallet(wallet_type_id);