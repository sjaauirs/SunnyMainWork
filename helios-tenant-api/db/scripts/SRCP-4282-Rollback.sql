UPDATE tenant.tenant
SET tenant_attr = tenant_attr #- '{ux,themeColors,headerBgColor}'
WHERE delete_nbr = 0
  AND tenant_attr IS NOT NULL
  AND tenant_attr->'ux' IS NOT NULL
  AND tenant_attr->'ux' != '{}'::jsonb
  AND tenant_attr->'ux'->'themeColors' ? 'headerBgColor';