namespace YTStdTenantPlatform.Domain.Enums;

/// <summary>支付渠道</summary>
public enum PaymentChannel
{
    /// <summary>支付宝</summary>
    Alipay,

    /// <summary>微信支付</summary>
    Wechat,

    /// <summary>银行转账</summary>
    BankTransfer,

    /// <summary>线下支付</summary>
    Offline,

    /// <summary>其他</summary>
    Other
}
