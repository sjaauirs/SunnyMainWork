-- Update the tenant attribute JSON to include the "autosweepSweepstakesReward" flag.
-- This ensures that all valid tenants have the "autosweepSweepstakesReward" property set to default value as false.
UPDATE tenant.tenant
SET tenant_attr = jsonb_set(
    tenant_attr,
    '{autosweepSweepstakesReward}',
    'false'::jsonb,
    true
)
WHERE delete_nbr = 0
	AND tenant_attr IS NOT NULL
	AND tenant_attr <> '{}'::jsonb
	AND NOT tenant_attr ? 'autosweepSweepstakesReward';
	