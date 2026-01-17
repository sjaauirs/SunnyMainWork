UPDATE tenant.tenant 
SET tenant_attr = jsonb_set(
    tenant_attr,
    '{disableMembershipDollars}',
    'true',
    true
)
WHERE tenant_code = 'ten-ecada21e57154928a2bb959e8365b8b4'
AND delete_nbr = 0;