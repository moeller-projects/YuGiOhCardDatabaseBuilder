using System;

namespace YuGiOhDatabaseBuilderV2
{
    public class ModuleAttribute : Attribute
    {

        public string Name { get; set; }

        public ModuleAttribute(string name)
            => Name = name;

        public ModuleAttribute() { }

    }
}
