using Autofac;
using Autofac.Extensions.DependencyInjection;
using JsonFlatFileDataStore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using YuGiOhDatabaseBuilderV2.Models;
using YuGiOhDatabaseBuilderV2.Modules;

namespace YuGiOhDatabaseBuilderV2
{
    internal class Program
    {
        private static void Main(string[] args) => new Program().MainAsync(args).GetAwaiter().GetResult();

        private IConfiguration _configuration;

        public async Task MainAsync(string[] args)
        {
            _configuration = BuildConfig(args);

            var services = ConfigureServices();

            try
            {
                CommandLine.Parser.Default.ParseArguments<BuildArguments>(args)
                    .WithParsed(Build)
                    .WithNotParsed(Errors);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Environment.Exit(1);
            }

#if DEBUG
            Console.ReadLine();
#endif
        }

        private void Errors(IEnumerable<Error> errors)
        {
            foreach (var error in errors)
            {
                Console.WriteLine(error);
            }
            Environment.Exit(1);
        }

        private void Build(BuildArguments arguments)
        {
            var cards = new CardList();

            var modules = Assembly.GetEntryAssembly().GetTypes()
                .Where(type => type.CustomAttributes.Any(attribute => attribute.AttributeType == typeof(ModuleAttribute)))
                .Select(moduleType => Task.Run(async () =>
                {
                    var module = Activator.CreateInstance(moduleType) as ModuleBase;
                    var moduleInfo = await module.RunAsync();
                    cards.AddRange(moduleInfo.Cards.Where(w => w != null));
                }))
                .ToArray();

            Task.WaitAll(modules);

            CreateDatabaseAsync(Path.Combine(arguments.DatabasePath, arguments.DatabaseName), cards).ConfigureAwait(true);
        }

        private async Task CreateDatabaseAsync(string path, IEnumerable<Card> cards)
        {
            var sqliteConnection = path;
            var allCards = cards as Card[] ?? cards.ToArray();
            using (var database = new SQLiteConnection(sqliteConnection))
            {
                database.CreateTable<Card>();
                database.InsertAll(allCards.Distinct());
            }

            //using (var store = new DataStore("C:\\Temp\\temp.json", true, "id"))
            //{
            //    await store.GetCollection<Card>().InsertManyAsync(allCards);
            //}
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services
                // Logging
                .AddLogging(c => c
                                .AddConsole())
                // Extra
                .AddSingleton(_configuration);

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
