using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace YuGiOhCardDatabaseBuilder.Models.SQLiteDbContext
{
    public class SqliteContext : DbContext
    {
        public DbSet<Card> Cards { get; set; }

        public DbSet<Booster> Boosters { get; set; }

        public SqliteContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            Configure();
        }

        public SqliteContext(DbConnection connection, bool contextOwnsConnection)
            : base(connection, contextOwnsConnection)
        {
            Configure();
        }

        private void Configure()
        {
            Configuration.ProxyCreationEnabled = true;
            Configuration.LazyLoadingEnabled = true;
        }

        //public SqliteContext(string connectionString) : base(new SQLiteConnection(connectionString), false) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Chinook Database does not pluralize table names
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}
