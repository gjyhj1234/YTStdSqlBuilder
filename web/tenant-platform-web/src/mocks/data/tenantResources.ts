export const mockTenantResourceQuotas = [
  {
    Id: 1,
    TenantRefId: 1,
    QuotaType: 'MaxUsers',
    QuotaLimit: 100,
    WarningThreshold: 80,
    ResetCycle: 'None',
    CreatedAt: '2025-01-15T10:00:00Z',
  },
  {
    Id: 2,
    TenantRefId: 1,
    QuotaType: 'StorageGB',
    QuotaLimit: 50,
    WarningThreshold: 40,
    ResetCycle: 'Monthly',
    CreatedAt: '2025-01-15T10:00:00Z',
  },
  {
    Id: 3,
    TenantRefId: 2,
    QuotaType: 'ApiCallsPerDay',
    QuotaLimit: 10000,
    WarningThreshold: 8000,
    ResetCycle: 'Daily',
    CreatedAt: '2025-02-01T08:30:00Z',
  },
]
