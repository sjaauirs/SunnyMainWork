UPDATE tenant.tenant
SET tenant_attr = tenant_attr #- '{ux,cardActivationBannerColors}'
WHERE tenant_code = '<TenantCode>';  -- Replace with actual tenant_code both HAP and KP
