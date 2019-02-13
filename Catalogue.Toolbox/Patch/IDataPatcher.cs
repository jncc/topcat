using System.Collections.Generic;
using Catalogue.Data.Write;

namespace Catalogue.Toolbox.Patch
{
    interface IDataPatcher
    {
        List<RecordServiceResult> Patch();
    }
}
