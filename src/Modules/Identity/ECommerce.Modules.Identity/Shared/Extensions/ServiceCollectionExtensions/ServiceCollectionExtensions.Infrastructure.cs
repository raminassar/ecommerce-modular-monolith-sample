using BuildingBlocks.Caching.InMemory;
using BuildingBlocks.Core.Caching;
using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Core.Registrations;
using BuildingBlocks.Email;
using BuildingBlocks.Email.Options;
using BuildingBlocks.Logging;
using BuildingBlocks.Security;
using BuildingBlocks.Security.Jwt;
using BuildingBlocks.Validation;
using BuildingBlocks.Web.Extensions.ServiceCollectionExtensions;

namespace ECommerce.Modules.Identity.Shared.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllersAsServices();

        services.AddCqrs(doMoreActions: s =>
        {
            s.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>))
                .AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(StreamRequestValidationBehavior<,>))
                .AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(StreamLoggingBehavior<,>))
                .AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(StreamCachingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(InvalidateCachingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(EfTxBehavior<,>));
        });

        services.AddCore(configuration, Assembly.GetExecutingAssembly());

        services.AddEmailService(configuration, $"{IdentityModuleConfiguration.ModuleName}:{nameof(EmailOptions)}");

        services.AddCustomValidators(Assembly.GetExecutingAssembly());

        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddJwt(configuration);

        services.AddInMemoryMessagePersistence();
        services.AddInMemoryCommandScheduler();
        services.AddInMemoryBroker(configuration);

        // services.AddMonitoring(healthChecksBuilder =>
        // {
        //     var postgresOptions = configuration.GetOptions<PostgresOptions>(
        //         $"{IdentityModuleConfiguration.ModuleName}:{nameof(PostgresOptions)}");
        //     Guard.Against.Null(postgresOptions, nameof(postgresOptions));
        //
        //     healthChecksBuilder.AddNpgSql(
        //         postgresOptions.ConnectionString,
        //         name: "Identity-Module-Postgres-Check",
        //         tags: new[] {"identity-postgres"});
        // });

        services.AddCustomInMemoryCache(configuration)
            .AddCachingRequestPolicies(Assembly.GetExecutingAssembly());

        services.AddSingleton<ILoggerFactory>(new Serilog.Extensions.Logging.SerilogLoggerFactory());

        return services;
    }
}
