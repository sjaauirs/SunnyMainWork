UPDATE tenant.tenant
SET tenant_attr = jsonb_set(
    tenant_attr,
    '{ux,themeColors,headerBgColor}',
    '"#0D1C3D"',
    true
)
WHERE delete_nbr = 0
  AND tenant_attr IS NOT NULL
  AND tenant_attr->'ux' IS NOT NULL
  AND tenant_attr->'ux' != '{}'::jsonb
  AND tenant_attr->'ux'->'themeColors' IS NOT NULL;