using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOMPPDMLoaderLibrary
{
    public interface IDataTransfer
    {
        Task Transferdata(string connectionString, string datatype);
    }
}
