using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOMPPDMLoaderLibrary.Models
{
    public class DeviationReferenceTables
    {
        public List<ReferenceTable> RefTables { get; }
        public DeviationReferenceTables()
        {
            this.RefTables = new List<ReferenceTable>()
            {
                new ReferenceTable()
                { KeyAttribute = "SOURCE", Table = "R_SOURCE", ValueAttribute= "LONG_NAME"},
                new ReferenceTable()
                { KeyAttribute = "NORTH_TYPE", Table = "R_NORTH", ValueAttribute= "LONG_NAME"},
                new ReferenceTable()
                { KeyAttribute = "COMPUTE_METHOD", Table = "R_DIR_SRVY_COMPUTE", ValueAttribute= "LONG_NAME"},
            };
        }
    }
}
