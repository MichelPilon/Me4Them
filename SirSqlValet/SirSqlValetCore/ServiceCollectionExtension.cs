using Microsoft.Extensions.DependencyInjection;
using SirSqlValetCore.App;
using SirSqlValetCore.Di;
using SirSqlValetCore.Integration;
using SirSqlValetCore.Integration.ObjectExplorer;

namespace SirSqlValetCore
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddSirSqlValetCoreServices(this IServiceCollection services)
        {
            services.AddSingleton<IObjectExplorerInteraction, ObjectExplorerInteraction>();
            services.AddSingleton<IServiceCacheIntegration, ServiceCacheIntegration>();

            return services;
        }
    }
}
