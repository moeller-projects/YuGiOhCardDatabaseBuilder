using System;

namespace YuGiOhDatabaseBuilderV2
{
    public class DataStoreAttribute : Attribute
    {

        public string Name { get; set; }

        public DataStoreAttribute(string name)
            => Name = name;

        public DataStoreAttribute() { }

    }
}
