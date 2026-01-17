-- Update all records in the person_role table where customer_code is 'All' and set it to 'ALL' for consistency in casing.
UPDATE huser.person_role SET customer_code = 'ALL' WHERE customer_code = 'All';

-- Update all records in the person_role table where sponsor_code is 'All' and set it to 'ALL' for uniform data representation.
UPDATE huser.person_role SET sponsor_code = 'ALL' WHERE sponsor_code = 'All';

-- Update all records in the person_role table where tenant_code is 'All' and set it to 'ALL' to maintain consistent data formatting.
UPDATE huser.person_role SET tenant_code = 'ALL' WHERE tenant_code = 'All';
