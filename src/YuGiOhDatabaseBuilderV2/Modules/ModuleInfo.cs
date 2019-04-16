using System.Collections.Generic;
using YuGiOhDatabaseBuilderV2.Models;

namespace YuGiOhDatabaseBuilderV2.Modules
{
    public class ModuleInfo
    {
        public IEnumerable<Card> Cards { get; set; }
    }
}