using GOMPPDMLoaderLibrary.Data;
using GOMPPDMLoaderLibrary.DataAccess;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GOMPPDMLoaderLibrary
{
    public class DataTransfer : IDataTransfer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DataTransfer> _log;
        private readonly IDataAccess _da;
        private readonly IWellData _wellData;
        private readonly HttpClient _httpClient;

        public DataTransfer(ILogger<DataTransfer> log, IConfiguration configuration, IDataAccess da,
            IWellData wellData)
        {
            _log = log;
            _configuration = configuration;
            _wellData = wellData;
            _da = da;
            //_httpClient = httpClient;
        }

        public async Task Transferdata(string connectionString, string datatype)
        {
            try
            {
                _log.LogInformation("Start Data Transfer");
                //IDataAccess da = new DapperDataAccess();
                //IWellData wellData = new Welldata(da, _log);
                _log.LogInformation("Start Data Copy");

                if (datatype.Equals("Wellbore", StringComparison.OrdinalIgnoreCase))
                {
                    await _wellData.CopyCompanyData(connectionString);
                    await _wellData.CopyWellbores(connectionString);
                }
                else if (datatype.Equals("Deviations", StringComparison.OrdinalIgnoreCase))
                {
                    await _wellData.CopyDeviations(connectionString);
                }
                else
                {
                    _log.LogWarning("Unknown datatype: {DataType}", datatype);
                }

                
                _log.LogInformation("Data has been Copied");
            }
            catch (Exception ex)
            {
                string errors = "Error transferring data: " + ex.ToString();
                _log.LogError(errors);
            }
        }
    }
}
