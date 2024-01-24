namespace SirSqlValetCommands
{
    using Microsoft.Extensions.DependencyInjection;
    using SirSqlValetCommands.UI;

    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddSirSqlValetCommandsServices(this IServiceCollection services)
        {
            services.AddSingleton<CommandsUI>();
            services.AddSingleton<CommandsPlugin>();
            
            services.AddTransient<ExportDocumentsControlVM>();
            return services;
        }
    }
}
