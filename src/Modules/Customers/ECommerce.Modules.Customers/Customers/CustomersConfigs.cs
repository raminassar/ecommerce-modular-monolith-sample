using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Web.Module;
using ECommerce.Modules.Customers.Customers.Data;

namespace ECommerce.Modules.Customers.Customers;

internal static class CustomersConfigs
{
    public const string Tag = "Customers";
    public const string CustomersPrefixUri = $"{CustomersModuleConfiguration.CustomerModulePrefixUri}";

    internal static IServiceCollection AddCustomersServices(this IServiceCollection services)
    {
        services.AddScoped<IDataSeeder, CustomersDataSeeder>();

        return services;
    }

    internal static IEndpointRouteBuilder MapCustomersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Routes mapped by conventions with implementing `IMinimalEndpointDefinition` but we can map them here without convention.
        return endpoints;
    }
}
