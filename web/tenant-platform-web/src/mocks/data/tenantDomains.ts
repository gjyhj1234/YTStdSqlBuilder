export const mockTenantDomains = [
  {
    Id: 1,
    TenantRefId: 1,
    Domain: 'huaxia.ytstd.com',
    DomainType: 'SubDomain',
    IsPrimary: true,
    VerificationStatus: 'Verified',
    CreatedAt: '2025-01-15T10:00:00Z',
  },
  {
    Id: 2,
    TenantRefId: 1,
    Domain: 'www.huaxia-tech.com',
    DomainType: 'CustomDomain',
    IsPrimary: false,
    VerificationStatus: 'Pending',
    CreatedAt: '2025-02-01T08:00:00Z',
  },
  {
    Id: 3,
    TenantRefId: 2,
    Domain: 'yunhai.ytstd.com',
    DomainType: 'SubDomain',
    IsPrimary: true,
    VerificationStatus: 'Verified',
    CreatedAt: '2025-02-01T08:30:00Z',
  },
]
