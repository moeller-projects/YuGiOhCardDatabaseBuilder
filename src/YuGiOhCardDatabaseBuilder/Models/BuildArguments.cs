using CommandLine;

namespace YuGiOhCardDatabaseBuilder.Models
{
    [Verb("build", HelpText = "build a sqlite db with ygo card data")]
    public class BuildArguments
    {
        [Option('f', "databasepath", Default = ".", HelpText = "path to database file", Required = false)]
        public string DatabasePath { get; set; }

        [Option('n', "databasename", Default = "ygo.db", HelpText = "database name", Required = false)]
        public string DatabaseName { get; set; }
    }
}
