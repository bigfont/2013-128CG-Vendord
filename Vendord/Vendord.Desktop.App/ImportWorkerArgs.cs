using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vendord.Desktop.App
{
    public class ImportWorkerArgs
    {
        internal enum ImportType
        {
            Product,
            Vendor
        }

        internal string FilePath { get; set; }

        internal ImportType Importing { get; set; }
    }
}
