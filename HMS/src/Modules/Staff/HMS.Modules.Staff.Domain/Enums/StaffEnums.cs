namespace HMS.Modules.Staff.Domain.Enums;

public enum EmploymentType
{
    Monthly,
    Daily,
    Hourly,
    Contract
}

public static class EmploymentTypeMapper
{
    public static string ToDatabaseValue(this EmploymentType type) => type switch
    {
        EmploymentType.Monthly => "MONTHLY",
        EmploymentType.Daily => "DAILY",
        EmploymentType.Hourly => "HOURLY",
        EmploymentType.Contract => "CONTRACT",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported employment type.")
    };
}

public enum StaffMemberStatus
{
    Active,
    OnLeave,
    Left,
    Suspended
}

public static class StaffMemberStatusMapper
{
    public static string ToDatabaseValue(this StaffMemberStatus status) => status switch
    {
        StaffMemberStatus.Active => "ACTIVE",
        StaffMemberStatus.OnLeave => "ON_LEAVE",
        StaffMemberStatus.Left => "LEFT",
        StaffMemberStatus.Suspended => "SUSPENDED",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported staff status.")
    };
}

public enum StaffPaymentMethod
{
    Cash,
    BankTransfer,
    Card,
    Cheque,
    Other
}

public static class StaffPaymentMethodMapper
{
    public static string ToDatabaseValue(this StaffPaymentMethod method) => method switch
    {
        StaffPaymentMethod.Cash => "CASH",
        StaffPaymentMethod.BankTransfer => "BANK_TRANSFER",
        StaffPaymentMethod.Card => "CARD",
        StaffPaymentMethod.Cheque => "CHEQUE",
        StaffPaymentMethod.Other => "OTHER",
        _ => throw new ArgumentOutOfRangeException(nameof(method), method, "Unsupported payment method.")
    };
}
