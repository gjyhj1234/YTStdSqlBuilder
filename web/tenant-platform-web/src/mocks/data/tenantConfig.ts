export const mockTenantSystemConfigs = [
  {
    Id: 1,
    TenantRefId: 1,
    SystemName: '华夏科技管理平台',
    LogoUrl: '/uploads/logos/huaxia.png',
    SystemTheme: 'light',
    DefaultLanguage: 'zh-CN',
    DefaultTimezone: 'Asia/Shanghai',
  },
  {
    Id: 2,
    TenantRefId: 2,
    SystemName: '云海数据服务中心',
    LogoUrl: '/uploads/logos/yunhai.png',
    SystemTheme: 'dark',
    DefaultLanguage: 'zh-CN',
    DefaultTimezone: 'Asia/Shanghai',
  },
]

export const mockTenantFeatureFlags = [
  {
    Id: 1,
    TenantRefId: 1,
    FeatureKey: 'enable_advanced_report',
    FeatureName: '高级报表',
    Enabled: true,
    Description: '启用高级数据分析报表功能',
    CreatedAt: '2025-01-20T00:00:00Z',
  },
  {
    Id: 2,
    TenantRefId: 1,
    FeatureKey: 'enable_api_access',
    FeatureName: 'API接入',
    Enabled: true,
    Description: '允许通过API访问平台服务',
    CreatedAt: '2025-01-20T00:00:00Z',
  },
  {
    Id: 3,
    TenantRefId: 2,
    FeatureKey: 'enable_advanced_report',
    FeatureName: '高级报表',
    Enabled: false,
    Description: '启用高级数据分析报表功能',
    CreatedAt: '2025-02-05T00:00:00Z',
  },
]

export const mockTenantParameters = [
  {
    Id: 1,
    TenantRefId: 1,
    ParamKey: 'max_login_attempts',
    ParamValue: '5',
    ParamType: 'Integer',
    Description: '最大登录尝试次数',
    CreatedAt: '2025-01-20T00:00:00Z',
  },
  {
    Id: 2,
    TenantRefId: 1,
    ParamKey: 'session_timeout_minutes',
    ParamValue: '30',
    ParamType: 'Integer',
    Description: '会话超时时间（分钟）',
    CreatedAt: '2025-01-20T00:00:00Z',
  },
  {
    Id: 3,
    TenantRefId: 2,
    ParamKey: 'max_login_attempts',
    ParamValue: '3',
    ParamType: 'Integer',
    Description: '最大登录尝试次数',
    CreatedAt: '2025-02-05T00:00:00Z',
  },
]
