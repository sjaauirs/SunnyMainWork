UPDATE tenant.tenant
SET tenant_attr = jsonb_set(
    tenant_attr,
    '{ux,cardActivationBannerColors}',
    '{
      "bannerBackgroundColor": "#E9EBEE",
      "bannerBackgroundColorOnActivation": "#57A635",
      "bannerTextColor": "#0D1C3D",
      "bannerTextColorOnActivation": "#FFFFFF"
    }',
    true
)
WHERE  tenant_code = '<KP-TenantCode>' and delete_nbr = 0; -- Replace <KP TenantCode>
