UPDATE tenant.tenant
SET tenant_attr = jsonb_set(
    tenant_attr,
    '{ux,cardActivationBannerColors}',
    '{
      "bannerBackgroundColor": "#E3E4E5",
      "bannerBackgroundColorOnActivation": "#E3E4E5",
      "bannerTextColor": "#181D27",
      "bannerTextColorOnActivation": "#181D27"
    }',
    true
)
WHERE -- 👇 Add your condition here (e.g., for specific tenant_code)
 tenant_code = '<HAP-TenantCode>' and delete_nbr = 0; -- Replace <HAP TenantCode>, ex: ten-b4e920d3f6f74496ab533d1a9a8ef9e4