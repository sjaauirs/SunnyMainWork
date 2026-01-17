-- Rollback: Remove 'autosweepSweepstakesReward' flag from root level of tenant_attr
UPDATE tenant.tenant
SET tenant_attr = tenant_attr - 'autosweepSweepstakesReward'
WHERE tenant_attr IS NOT NULL
  AND tenant_attr <> '{}'::jsonb
  AND tenant_attr ? 'autosweepSweepstakesReward'
  AND delete_nbr = 0;
  
  