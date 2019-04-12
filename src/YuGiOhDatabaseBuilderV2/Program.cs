using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using YuGiOhDatabaseBuilderV2.Modules;

namespace YuGiOhDatabaseBuilderV2
{
    internal class Program
    {
        private static void Main(string[] args) => new Program().MainAsync(args).GetAwaiter().GetResult();

        private IConfiguration Configuration;

        public async Task MainAsync(string[] args)
        {
            Configuration = BuildConfig(args);

            var services = ConfigureServices();

            var modules = Assembly.GetEntryAssembly().GetTypes()
                .Where(type => type.CustomAttributes.Any(attribute => attribute.AttributeType == typeof(ModuleAttribute)));

            foreach (var moduleType in modules)
            {
                var module = Activator.CreateInstance(moduleType) as ModuleBase;
                var moduleInfo = await module.RunAsync();
            }

            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services
                // Logging
                .AddLogging(c => c
                                .AddConsole())
                // Extra
                .AddSingleton(Configuration);

            var container = new ContainerBuilder();

            container.Populate(services);

            return new AutofacServiceProvider(container.Build());
        }

        private static IConfiguration BuildConfig(string[] args)
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                //.AddJsonFile("config.json", optional: false, reloadOnChange: true)
                .Build();
        }
    }
}
