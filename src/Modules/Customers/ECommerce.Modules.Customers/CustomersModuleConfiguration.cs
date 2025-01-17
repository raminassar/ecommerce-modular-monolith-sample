using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Core;
using BuildingBlocks.Core.Messaging.Extensions;
using ECommerce.Modules.Customers.Customers;
using ECommerce.Modules.Customers.RestockSubscriptions;
using ECommerce.Modules.Customers.Shared.Extensions.ApplicationBuilderExtensions;
using ECommerce.Modules.Customers.Shared.Extensions.ServiceCollectionExtensions;

namespace ECommerce.Modules.Customers;

public class CustomersModuleConfiguration : IModuleDefinition
{
    public const string CustomerModulePrefixUri = "api/v1/customers";
    public const string ModuleName = "Customers";

    public void AddModuleServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddStorage(configuration);

        services.AddCustomersServices();
        services.AddRestockSubscriptionServices();
    }

    public async Task ConfigureModule(
        IApplicationBuilder app,
        IConfiguration configuration,
        ILogger logger,
        IWebHostEnvironment environment)
    {
        ServiceActivator.Configure(app.ApplicationServices);

        app.SubscribeAllMessageFromAssemblyOfType<CustomersRoot>();

        await app.ApplyDatabaseMigrations(logger);
        await app.SeedData(logger, environment);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapCustomersEndpoints();
        endpoints.MapRestockSubscriptionsEndpoints();

        endpoints.MapGet("customers", (HttpContext context) =>
        {
            var requestId = context.Request.Headers.TryGetValue("X-Request-Id", out var requestIdHeader)
                ? requestIdHeader.FirstOrDefault()
                : string.Empty;

            return $"Customers Service Apis, RequestId: {requestId}";
        }).ExcludeFromDescription();
    }
}
