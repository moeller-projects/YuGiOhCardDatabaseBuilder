using System.Collections.Generic;
using YuGiOhCardDatabaseBuilder.Models;

namespace YuGiOhDatabaseBuilderV2.Modules
{
    public class ModuleInfo
    {
        public IEnumerable<Card> Cards { get; set; }
    }
}