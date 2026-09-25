namespace HMS.Modules.Identity.Application.Authentication;

public static class IdentityRoles
{
    public const string PlatformAdmin = "PLATFORM_ADMIN";
    public const string HotelAdmin = "HOTEL_ADMIN";
    public const string Manager = "MANAGER";
    public const string FrontDesk = "FRONT_DESK";
    public const string Housekeeping = "HOUSEKEEPING";
    public const string Restaurant = "RESTAURANT";
    public const string Bar = "BAR";
    public const string Cashier = "CASHIER";

    public static readonly string[] AdminLoginRoles =
    [
        HotelAdmin,
        Manager
    ];

    public static readonly string[] StaffLoginRoles =
    [
        FrontDesk,
        Housekeeping,
        Restaurant,
        Bar,
        Cashier
    ];

    public static readonly string[] StaffRegisterRoles =
    [
        FrontDesk,
        Housekeeping,
        Restaurant,
        Bar,
        Cashier
    ];

    public static string ToHotelAccessRole(string identityRole) => identityRole switch
    {
        PlatformAdmin => PlatformAdmin,
        HotelAdmin => "PROPERTY_ADMIN",
        Manager => Manager,
        _ => identityRole
    };
}
