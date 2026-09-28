namespace HMS.Modules.Payments.Domain.Enums;

public enum BookingPaymentMethod
{
    Cash,
    BankTransfer,
    Card,
    OnlineGateway,
    Cheque,
    Other
}

public static class BookingPaymentMethodMapper
{
    public static string ToDatabaseValue(this BookingPaymentMethod method) => method switch
    {
        BookingPaymentMethod.Cash => "CASH",
        BookingPaymentMethod.BankTransfer => "BANK_TRANSFER",
        BookingPaymentMethod.Card => "CARD",
        BookingPaymentMethod.OnlineGateway => "ONLINE_GATEWAY",
        BookingPaymentMethod.Cheque => "CHEQUE",
        BookingPaymentMethod.Other => "OTHER",
        _ => throw new ArgumentOutOfRangeException(nameof(method), method, "Unsupported payment method.")
    };
}

public enum BookingPaymentType
{
    Deposit,
    Payment,
    Adjustment
}

public static class BookingPaymentTypeMapper
{
    public static string ToDatabaseValue(this BookingPaymentType type) => type switch
    {
        BookingPaymentType.Deposit => "DEPOSIT",
        BookingPaymentType.Payment => "PAYMENT",
        BookingPaymentType.Adjustment => "ADJUSTMENT",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported payment type.")
    };
}

public enum BookingPaymentStatus
{
    Pending,
    Completed,
    Failed,
    Cancelled
}

public static class BookingPaymentStatusMapper
{
    public static string ToDatabaseValue(this BookingPaymentStatus status) => status switch
    {
        BookingPaymentStatus.Pending => "PENDING",
        BookingPaymentStatus.Completed => "COMPLETED",
        BookingPaymentStatus.Failed => "FAILED",
        BookingPaymentStatus.Cancelled => "CANCELLED",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported payment status.")
    };
}
