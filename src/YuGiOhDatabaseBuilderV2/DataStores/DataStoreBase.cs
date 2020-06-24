using System.Collections.Generic;
using System.Threading.Tasks;
using YuGiOhDatabaseBuilderV2.Models;

namespace YuGiOhDatabaseBuilderV2.DataStores
{
    public abstract class DataStoreBase
    {
        public string DataStoreName { get; private set; }

        public DataStoreBase(string dataStoreName)
        {
            DataStoreName = dataStoreName;
        }

        public abstract Task RunAsync(IEnumerable<Card> cards);
    }
}