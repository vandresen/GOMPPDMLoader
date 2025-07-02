using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOMPPDMLoaderLibrary.Data
{
    public interface IWellData
    {
        Task CopyWellbores(string connectionString);
        Task CopyCompanyData(string connectionString);
        Task CopyDeviations(string connectionString);
    }
}
