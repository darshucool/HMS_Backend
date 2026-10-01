namespace HMS.Modules.Staff.Application.Common;

public static class StaffProfileRules
{
    public static string? Validate(
        string employeeNumber,
        string firstName,
        string? lastName,
        string? phone,
        string? email,
        decimal? basicSalary,
        decimal? dailyRate,
        decimal? hourlyRate,
        DateOnly? joinedDate,
        DateOnly? leftDate)
    {
        if (string.IsNullOrWhiteSpace(employeeNumber) || employeeNumber.Trim().Length > 50)
            return "Employee number is required and cannot exceed 50 characters.";

        if (string.IsNullOrWhiteSpace(firstName) || firstName.Trim().Length > 100)
            return "First name is required and cannot exceed 100 characters.";

        if (lastName is { Length: > 100 })
            return "Last name cannot exceed 100 characters.";

        if (phone is { Length: > 30 })
            return "Phone cannot exceed 30 characters.";

        if (email is { Length: > 254 })
            return "Email cannot exceed 254 characters.";

        if (basicSalary < 0 || dailyRate < 0 || hourlyRate < 0)
            return "Salary and rates cannot be negative.";

        if (leftDate is DateOnly left && joinedDate is DateOnly joined && left < joined)
            return "Left date cannot be before the joined date.";

        return null;
    }

    public static string? TrimOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
