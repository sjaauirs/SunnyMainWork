-- Update tenant_attr to add "benefitsCardArtUrl", Please ensure to update the URL and Tenant Code based on the environment before executing the script.
-- app-static image URLS 
-- DEV: https://app-static.dev.sunnyrewards.com/public/images/credit-card.png
-- QA: https://app-static.qa.sunnyrewards.com/public/images/credit-card.png
-- UAT: https://app-static.uat.sunnybenefits.com/public/images/credit-card.png
-- INTEG: https://app-static.integ.sunnyrewards.com/public/images/credit-card.png

UPDATE tenant.tenant SET tenant_attr = jsonb_set(tenant_attr,
'{benefitsCardArtUrl}', '"https://app-static.dev.sunnyrewards.com/public/images/credit-card.png"', true)
WHERE tenant_code = 'ten-ecada21e57154928a2bb959e8365b8b4' AND delete_nbr = 0;

-- Rollback tenant_attr JSON changes to remove "benefitsCardArtUrl"
UPDATE tenant.tenant
SET tenant_attr = tenant_attr - 'benefitsCardArtUrl'
WHERE tenant_code = 'ten-ecada21e57154928a2bb959e8365b8b4'
AND delete_nbr = 0;


