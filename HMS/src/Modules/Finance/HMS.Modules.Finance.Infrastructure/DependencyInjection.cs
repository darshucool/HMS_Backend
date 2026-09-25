using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Infrastructure.Persistence;
using HMS.Modules.Finance.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HMS.Modules.Finance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFinanceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IFinanceDbConnectionFactory, FinanceDbConnectionFactory>();
        services.AddScoped<IFinancePropertyAccess, FinancePropertyAccessRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IUtilityBillRepository, UtilityBillRepository>();
        services.AddScoped<IOtherIncomeRepository, OtherIncomeRepository>();
        services.AddScoped<IExpenseCategoryRepository, ExpenseCategoryRepository>();
        return services;
    }
}
