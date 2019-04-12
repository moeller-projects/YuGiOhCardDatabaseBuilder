using System;
using System.Collections.Generic;
using System.Text;

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
