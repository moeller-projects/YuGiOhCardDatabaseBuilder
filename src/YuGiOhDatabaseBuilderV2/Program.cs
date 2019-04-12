using Autofac;
using Autofac.Extensions.DependencyInjection;
using JsonFlatFileDataStore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using YuGiOhCardDatabaseBuilder.Models;
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

            var cards = new List<Card>();

            var modules = Assembly.GetEntryAssembly().GetTypes()
                .Where(type => type.CustomAttributes.Any(attribute => attribute.AttributeType == typeof(ModuleAttribute)))
                .Select(moduleType => Task.Run(async () =>
                {
                    var module = Activator.CreateInstance(moduleType) as ModuleBase;
                    var moduleInfo = await module.RunAsync();
                    cards.AddRange(moduleInfo.Cards);
                }))
                .ToArray();

            Task.WaitAll(modules);

            await CreateDatabaseAsync(cards);
        }

        private async Task CreateDatabaseAsync(IEnumerable<Card> cards)
        {
            var sqliteConnection = Path.Combine("C:\\Temp\\temp.db");
            using (var database = new SQLiteConnection(sqliteConnection))
            {
                database.CreateTable<Card>();
                database.InsertAll(cards.Distinct());
            }

            using (var store = new DataStore("C:\\Temp\\temp.json", true, "id"))
            {
                await store.GetCollection<Card>().InsertManyAsync(cards);
            }
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
