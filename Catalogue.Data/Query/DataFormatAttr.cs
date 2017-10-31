using System;

namespace Catalogue.Data.Query
{
    class DataFormatAttr : Attribute
    {
        public string Name { get; private set; }
        public DataFormatGroups Group { get; private set; }

        internal DataFormatAttr(string name, DataFormatGroups group)
        {
            Name = name;
            Group = group;
        }
    }
}
