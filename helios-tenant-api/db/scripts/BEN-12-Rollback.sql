-- =========================================================================================
-- Rollback Purpose:
-- Deletes the inserted record from `notification.tenant_notification_channel`
-- for the given tenant_code and config_json.
--
-- Jira Ticket: BEN-12
-- =========================================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TenantCode>'; -- Example: ten-b4e920d3f6f74496ab533d1a9a8ef9e4
    v_config_json JSONB := '{
      "ChannelConfig": [
        {
          "Enabled": true,
          "Supported": true,
          "ChannelType": "EMAIL",
          "ConsumerDefaultEnabled": false
        },
        {
          "Enabled": true,
          "Supported": true,
          "ChannelType": "SMS",
          "ConsumerDefaultEnabled": false
        }
      ]
    }'::jsonb; 
BEGIN
    DELETE FROM notification.tenant_notification_channel
    WHERE tenant_code = v_tenant_code
      AND config_json = v_config_json
      AND delete_nbr = 0;
END $$;
