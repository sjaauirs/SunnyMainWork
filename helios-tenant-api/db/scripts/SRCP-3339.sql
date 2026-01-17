BEGIN;

UPDATE tenant.tenant
SET tenant_attr = jsonb_set(
    jsonb_set(tenant_attr, '{pickAPurseOnboardingEnabled}', 'false'::jsonb, true),
    '{pickAPurseFundTransferEnabled}', 'false'::jsonb, true
)
WHERE tenant_attr IS NOT NULL
  AND tenant_attr <> '{}'::jsonb 
  AND delete_nbr=0;

COMMIT;
