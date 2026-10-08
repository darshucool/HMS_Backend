namespace HMS.Modules.Payments.Domain.Enums;

public enum BookingChargeCategory
{
    Food,
    Cooking,
    Laundry,
    Beverage,
    Transport,
    Activity,
    Damage,
    Other
}

public static class BookingChargeCategoryMapper
{
    public static string ToDatabaseValue(this BookingChargeCategory category) => category switch
    {
        BookingChargeCategory.Food => "FOOD",
        BookingChargeCategory.Cooking => "COOKING",
        BookingChargeCategory.Laundry => "LAUNDRY",
        BookingChargeCategory.Beverage => "BEVERAGE",
        BookingChargeCategory.Transport => "TRANSPORT",
        BookingChargeCategory.Activity => "ACTIVITY",
        BookingChargeCategory.Damage => "DAMAGE",
        BookingChargeCategory.Other => "OTHER",
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, "Unsupported charge category.")
    };

    public static BookingChargeCategory FromDatabaseValue(string value) => value switch
    {
        "FOOD" => BookingChargeCategory.Food,
        "COOKING" => BookingChargeCategory.Cooking,
        "LAUNDRY" => BookingChargeCategory.Laundry,
        "BEVERAGE" => BookingChargeCategory.Beverage,
        "TRANSPORT" => BookingChargeCategory.Transport,
        "ACTIVITY" => BookingChargeCategory.Activity,
        "DAMAGE" => BookingChargeCategory.Damage,
        "OTHER" => BookingChargeCategory.Other,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unsupported charge category.")
    };
}


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
    PartiallyRefunded,
    Refunded,
    Failed,
    Cancelled
}

public static class BookingPaymentStatusMapper
{
    public static string ToDatabaseValue(this BookingPaymentStatus status) => status switch
    {
        BookingPaymentStatus.Pending => "PENDING",
        BookingPaymentStatus.Completed => "COMPLETED",
        BookingPaymentStatus.PartiallyRefunded => "PARTIALLY_REFUNDED",
        BookingPaymentStatus.Refunded => "REFUNDED",
        BookingPaymentStatus.Failed => "FAILED",
        BookingPaymentStatus.Cancelled => "CANCELLED",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported payment status.")
    };
}
