namespace HMS.Modules.Finance.Domain.Enums;

public enum ExpenseGroup
{
    Staff,
    Utility,
    Food,
    Maintenance,
    Transport,
    Marketing,
    Supplies,
    Administration,
    Other
}

public static class ExpenseGroupMapper
{
    public static string ToDatabaseValue(this ExpenseGroup group) => group switch
    {
        ExpenseGroup.Staff => "STAFF",
        ExpenseGroup.Utility => "UTILITY",
        ExpenseGroup.Food => "FOOD",
        ExpenseGroup.Maintenance => "MAINTENANCE",
        ExpenseGroup.Transport => "TRANSPORT",
        ExpenseGroup.Marketing => "MARKETING",
        ExpenseGroup.Supplies => "SUPPLIES",
        ExpenseGroup.Administration => "ADMINISTRATION",
        ExpenseGroup.Other => "OTHER",
        _ => throw new ArgumentOutOfRangeException(nameof(group), group, "Unsupported expense group.")
    };

    public static ExpenseGroup FromDatabaseValue(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return value.Trim().ToUpperInvariant() switch
        {
            "STAFF" => ExpenseGroup.Staff,
            "UTILITY" => ExpenseGroup.Utility,
            "FOOD" => ExpenseGroup.Food,
            "MAINTENANCE" => ExpenseGroup.Maintenance,
            "TRANSPORT" => ExpenseGroup.Transport,
            "MARKETING" => ExpenseGroup.Marketing,
            "SUPPLIES" => ExpenseGroup.Supplies,
            "ADMINISTRATION" => ExpenseGroup.Administration,
            "OTHER" => ExpenseGroup.Other,
            _ => throw new ArgumentException($"Unsupported expense group '{value}'.", nameof(value))
        };
    }
}

public enum FinancePaymentMethod
{
    Cash,
    BankTransfer,
    Card,
    Cheque,
    Other
}

public static class FinancePaymentMethodMapper
{
    public static string ToDatabaseValue(this FinancePaymentMethod method) => method switch
    {
        FinancePaymentMethod.Cash => "CASH",
        FinancePaymentMethod.BankTransfer => "BANK_TRANSFER",
        FinancePaymentMethod.Card => "CARD",
        FinancePaymentMethod.Cheque => "CHEQUE",
        FinancePaymentMethod.Other => "OTHER",
        _ => throw new ArgumentOutOfRangeException(nameof(method), method, "Unsupported payment method.")
    };

    public static FinancePaymentMethod? FromDatabaseValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim().ToUpperInvariant().Replace("-", "_") switch
        {
            "CASH" => FinancePaymentMethod.Cash,
            "BANK_TRANSFER" => FinancePaymentMethod.BankTransfer,
            "CARD" => FinancePaymentMethod.Card,
            "CHEQUE" => FinancePaymentMethod.Cheque,
            "OTHER" => FinancePaymentMethod.Other,
            _ => throw new ArgumentException($"Unsupported payment method '{value}'.", nameof(value))
        };
    }
}
