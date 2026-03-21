export const mockTenantApiKeys = [
  {
    Id: 1,
    TenantRefId: 1,
    KeyName: '生产环境密钥',
    AccessKey: 'ak_huaxia_prod_xxxxxxxxxxxxx',
    Status: 'Active',
    ExpiresAt: '2026-01-15T00:00:00Z',
    CreatedAt: '2025-01-20T00:00:00Z',
  },
  {
    Id: 2,
    TenantRefId: 1,
    KeyName: '测试环境密钥',
    AccessKey: 'ak_huaxia_test_yyyyyyyyyyyyy',
    Status: 'Active',
    ExpiresAt: '2025-12-31T00:00:00Z',
    CreatedAt: '2025-01-20T00:00:00Z',
  },
  {
    Id: 3,
    TenantRefId: 2,
    KeyName: '默认密钥',
    AccessKey: 'ak_yunhai_default_zzzzzzzzzzz',
    Status: 'Disabled',
    ExpiresAt: '2025-08-01T00:00:00Z',
    CreatedAt: '2025-02-05T00:00:00Z',
  },
]

export const mockTenantWebhooks = [
  {
    Id: 1,
    TenantRefId: 1,
    WebhookUrl: 'https://hooks.huaxia-tech.com/callback',
    EventTypes: 'subscription.created,subscription.cancelled',
    Secret: 'whsec_xxxxxxxxxxxxxxxx',
    Status: 'Active',
    CreatedAt: '2025-02-01T00:00:00Z',
  },
  {
    Id: 2,
    TenantRefId: 2,
    WebhookUrl: 'https://api.yunhai-data.cn/webhook',
    EventTypes: 'invoice.paid,payment.success',
    Secret: 'whsec_yyyyyyyyyyyyyyyy',
    Status: 'Active',
    CreatedAt: '2025-02-10T00:00:00Z',
  },
]

export const mockWebhookDeliveryLogs = [
  {
    Id: 1,
    WebhookId: 1,
    EventType: 'subscription.created',
    RequestPayload: '{"event":"subscription.created","data":{"subscriptionId":1}}',
    ResponseStatus: 200,
    ResponseBody: '{"received":true}',
    DeliveredAt: '2025-02-01T10:00:01Z',
    CreatedAt: '2025-02-01T10:00:00Z',
  },
  {
    Id: 2,
    WebhookId: 2,
    EventType: 'invoice.paid',
    RequestPayload: '{"event":"invoice.paid","data":{"invoiceId":3}}',
    ResponseStatus: 200,
    ResponseBody: '{"ok":true}',
    DeliveredAt: '2025-02-10T09:01:00Z',
    CreatedAt: '2025-02-10T09:00:59Z',
  },
  {
    Id: 3,
    WebhookId: 1,
    EventType: 'subscription.cancelled',
    RequestPayload: '{"event":"subscription.cancelled","data":{"subscriptionId":3}}',
    ResponseStatus: 500,
    ResponseBody: 'Internal Server Error',
    DeliveredAt: '2025-06-01T16:01:00Z',
    CreatedAt: '2025-06-01T16:00:59Z',
  },
]
