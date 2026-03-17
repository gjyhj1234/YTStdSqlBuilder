namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>存储提供者类型</summary>
public enum StorageProviderType
{
    /// <summary>本地存储</summary>
    Local,

    /// <summary>AWS S3</summary>
    S3,

    /// <summary>阿里云OSS</summary>
    Oss,

    /// <summary>腾讯云COS</summary>
    Cos,

    /// <summary>MinIO</summary>
    Minio,

    /// <summary>其他</summary>
    Other
}
