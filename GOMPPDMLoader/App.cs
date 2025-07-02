using GOMPPDMLoaderLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOMPPDMLoader
{
    public class App
    {
        private readonly IDataTransfer _dataTransfer;

        public App(IDataTransfer dataTransfer)
        {
            _dataTransfer = dataTransfer;
        }

        public async Task Run(string connectionString, string datatype)
        {
            await _dataTransfer.Transferdata(connectionString, datatype);
        }
    }
}
