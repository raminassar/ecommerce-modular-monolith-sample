namespace ECommerce.Modules.Customers.RestockSubscriptions;

internal static class RestockSubscriptionsConfigs
{
    public const string Tag = "RestockSubscriptions";

    public const string RestockSubscriptionsUrl =
        $"{CustomersModuleConfiguration.CustomerModulePrefixUri}/restock-subscriptions";


    internal static IServiceCollection AddRestockSubscriptionServices(this IServiceCollection services)
    {
        return services;
    }

    internal static IEndpointRouteBuilder MapRestockSubscriptionsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Routes mapped by conventions with implementing `IMinimalEndpointDefinition` but we can map them here without convention.
        return endpoints;
    }
}
