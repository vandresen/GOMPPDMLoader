using CsvHelper;
using CsvHelper.Configuration;
using GOMPPDMLoaderLibrary.DataAccess;
using GOMPPDMLoaderLibrary.Extensions;
using GOMPPDMLoaderLibrary.Helpers;
using GOMPPDMLoaderLibrary.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using System.IO.Compression;
using System.Net.Http;

namespace GOMPPDMLoaderLibrary.Data
{
    public class Welldata : IWellData
    {
        private readonly string wellBoreUrl = @"https://www.data.boem.gov/Well/Files/5010fixed.zip";
        private readonly string companyUrl = @"https://www.data.boem.gov/Company/Files/compalldelimit.zip";
        private readonly string deviationsUrl = @"https://www.data.boem.gov/Well/Files/directdelimit.zip";
        private readonly IDataAccess _da;
        private readonly ILogger<DataTransfer> _log;
        private List<Company> _companies;
        private readonly HttpClient _httpClient;

        public Welldata(IDataAccess da, ILogger<DataTransfer> log, HttpClient httpClient)
        {
            _da = da;
            _log = log;
            _httpClient = httpClient;
        }

        public async Task CopyCompanyData(string connectionString)
        {
            using (HttpClient client = new HttpClient())
            {
                byte[] zipData = await client.GetByteArrayAsync(companyUrl);
                using (MemoryStream zipStream = new MemoryStream(zipData))
                {
                    using (ZipArchive archive = new ZipArchive(zipStream))
                    {
                        if (archive != null)
                        {
                            _log.LogInformation("Company sata has been downloaded from GOM website");
                            if (archive.Entries.Count > 0)
                            {
                                ZipArchiveEntry? entry = archive.GetEntry("compalldelimit.txt");
                                if (entry != null)
                                {
                                    _log.LogInformation("Start processing data");
                                    await ProcessCompanies(entry, connectionString);
                                }
                                else
                                {
                                    _log.LogError("File not found in the zip archive.");
                                }
                            }
                            else
                            {
                                _log.LogError("The zip archive is empty.");
                            }
                        }
                        else
                        {
                            _log.LogError("Failed to create the ZipArchive.");
                        }
                    }
                }
            }
        }

        public async Task CopyWellbores(string connectionString)
        {
            using (HttpClient client = new HttpClient())
            {
                byte[] zipData = await client.GetByteArrayAsync(wellBoreUrl);
                using (MemoryStream zipStream = new MemoryStream(zipData))
                {
                    using (ZipArchive archive = new ZipArchive(zipStream))
                    {
                        if (archive != null)
                        {
                            _log.LogInformation("Data has been downloaded from GOM website");
                            if (archive.Entries.Count > 0)
                            {
                                ZipArchiveEntry? entry = archive.GetEntry("5010.txt");
                                if (entry != null)
                                {
                                    _log.LogInformation("Start processing data");
                                    await ProcessWellbores(entry, connectionString);
                                }
                                else
                                {
                                    _log.LogError("File not found in the zip archive.");
                                }
                            }
                            else
                            {
                                _log.LogError("The zip archive is empty.");
                            }
                        }
                        else
                        {
                            _log.LogError("Failed to create the ZipArchive.");
                        }
                    }
                }
            }
        }

        private async Task ProcessCompanies(ZipArchiveEntry entry, string connectionString)
        {
            List<Company> records;
            using (Stream entryStream = entry.Open())
            using (var reader = new StreamReader(entryStream))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false }))
            {
                csv.Context.TypeConverterCache.AddConverter<DateTime>(new CustomDateTimeConverter());
                _companies = csv.GetRecords<Company>().ToList();
            }
        }

        private async Task ProcessWellbores(ZipArchiveEntry entry, string connectionString)
        {
            IEnumerable<TableSchema> tableAttributeInfo = await GetColumnInfo(connectionString, "WELL");
            TableSchema? dataProperty = tableAttributeInfo.FirstOrDefault(x => x.COLUMN_NAME == "WELL_NUM");
            int wellNumLength = dataProperty == null ? 4 : dataProperty.PRECISION;
            dataProperty = tableAttributeInfo.FirstOrDefault(x => x.COLUMN_NAME == "LEASE_NUM");
            int leaseNumLength = dataProperty == null ? 4 : dataProperty.PRECISION;
            dataProperty = tableAttributeInfo.FirstOrDefault(x => x.COLUMN_NAME == "OPERATOR");
            int operatorLength = dataProperty == null ? 4 : dataProperty.PRECISION;
            dataProperty = tableAttributeInfo.FirstOrDefault(x => x.COLUMN_NAME == "ASSIGNED_FIELD");
            int fieldLength = dataProperty == null ? 4 : dataProperty.PRECISION;
            dataProperty = tableAttributeInfo.FirstOrDefault(x => x.COLUMN_NAME == "WELL_NAME");
            int wellNameLength = dataProperty == null ? 4 : dataProperty.PRECISION;

            List<GOMWellbore> wells = new List<GOMWellbore>();
            string tempFilePath = Path.GetTempFileName();
            entry.ExtractToFile(tempFilePath, true);
            using (TextFieldParser parser = new TextFieldParser(tempFilePath))
            {
                parser.TextFieldType = FieldType.FixedWidth;
                parser.SetFieldWidths(12, 6, 8, 5, 8, 8, 10, 5, 5, 5, 5, 1, 5, 1, 2, 6, 5, 1, 5, 1,
                    2, 6, 8, 8, 1, 2, 3, 5, 16, 16, 16, 16, 10, 7, 2, 8);
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();

                    if (fields != null)
                    {
                        string operatorNumber = fields[3];
                        GOMWellbore well = new GOMWellbore
                        {
                            UWI = fields[0],
                            WELL_NAME = fields[1],
                            OPERATOR = fields[3].GetOperator(_companies),
                            ASSIGNED_FIELD = fields[4],
                            SPUD_DATE = fields[5].GetDateFromString("yyyyMMdd"),
                            LEASE_NUM = fields[6],
                            DEPTH_DATUM_ELEV = (decimal?)fields[7].GetDoubleFromString(),
                            DEPTH_DATUM = "KB",
                            FINAL_TD = (decimal?)fields[8].GetDoubleFromString(),
                            CURRENT_STATUS_DATE = fields[23].GetDateFromString("yyyyMMdd"),
                            CURRENT_STATUS = fields[26],
                            WATER_DEPTH = (decimal?)fields[27].GetDoubleFromString(),
                            SURFACE_LONGITUDE = fields[28].GetDoubleFromString(),
                            SURFACE_LATITUDE = fields[29].GetDoubleFromString(),
                            BOTTOM_HOLE_LONGITUDE = fields[30].GetDoubleFromString(),
                            BOTTOM_HOLE_LATITUDE = fields[31].GetDoubleFromString()
                        };
                        if (well.WELL_NAME.Length > wellNameLength)
                            well.WELL_NAME = well.WELL_NAME.Substring(0, wellNameLength);
                        if (well.OPERATOR.Length > operatorLength)
                            well.OPERATOR = well.OPERATOR.Substring(0, operatorLength);
                        if (well.ASSIGNED_FIELD.Length > fieldLength)
                            well.ASSIGNED_FIELD = well.ASSIGNED_FIELD.Substring(0, fieldLength);
                        if (well.LEASE_NUM.Length > fieldLength)
                            well.LEASE_NUM = well.LEASE_NUM.Substring(0, fieldLength);
                        wells.Add(well);
                    }
                }
            }

            await SaveWellboreRefData(wells, connectionString);
            await SaveWellbores(wells, connectionString);
        }

        public async Task SaveWellboreRefData(List<GOMWellbore> wellbores, string connectionString)
        {
            _log.LogInformation("Start saving reference data");
            Dictionary<string, List<ReferenceData>> refDict = new Dictionary<string, List<ReferenceData>>();
            ReferenceTables tables = new ReferenceTables();

            List<ReferenceData> refs = wellbores.Select(x => x.OPERATOR).Distinct().ToList().CreateReferenceDataObject();
            refDict.Add(tables.RefTables[0].Table, refs);
            refs = wellbores.Select(x => x.ASSIGNED_FIELD).Distinct().ToList().CreateReferenceDataObject();
            refDict.Add(tables.RefTables[1].Table, refs);
            refs = wellbores.Select(x => x.DEPTH_DATUM).Distinct().ToList().CreateReferenceDataObject();
            refDict.Add(tables.RefTables[2].Table, refs);
            refs = wellbores.Select(x => x.CURRENT_STATUS).Distinct().ToList().CreateReferenceDataObject();
            refDict.Add(tables.RefTables[3].Table, refs);

            foreach (var table in tables.RefTables)
            {
                refs = refDict[table.Table];
                string sql = "";
                if (table.Table == "R_WELL_STATUS")
                {
                    sql = $"IF NOT EXISTS(SELECT 1 FROM {table.Table} WHERE {table.KeyAttribute} = @Reference) " +
                $"INSERT INTO {table.Table} " +
                $"(STATUS_TYPE, {table.KeyAttribute}, {table.ValueAttribute}) " +
                $"VALUES('STATUS', @Reference, @Reference)";
                }
                else
                {
                    sql = $"IF NOT EXISTS(SELECT 1 FROM {table.Table} WHERE {table.KeyAttribute} = @Reference) " +
                $"INSERT INTO {table.Table} " +
                $"({table.KeyAttribute}, {table.ValueAttribute}) " +
                $"VALUES(@Reference, @Reference)";
                }
                await _da.SaveData(connectionString, refs, sql);
            }
        }

        private async Task SaveWellbores(IEnumerable<GOMWellbore> wellbores, string connectionString)
        {
            _log.LogInformation("Start saving wellbore data");
            string sql = "IF NOT EXISTS(SELECT 1 FROM WELL WHERE UWI = @UWI) " +
                "INSERT INTO WELL (UWI, WELL_NAME, OPERATOR, ASSIGNED_FIELD, SPUD_DATE, LEASE_NUM, " +
                "DEPTH_DATUM_ELEV, DEPTH_DATUM, CURRENT_STATUS_DATE, CURRENT_STATUS, WATER_DEPTH, " +
                "SURFACE_LONGITUDE, SURFACE_LATITUDE, BOTTOM_HOLE_LONGITUDE, BOTTOM_HOLE_LATITUDE) " +
                "VALUES(@UWI, @WELL_NAME, @OPERATOR, @ASSIGNED_FIELD, @SPUD_DATE, @LEASE_NUM, " +
                "@DEPTH_DATUM_ELEV, @DEPTH_DATUM, @CURRENT_STATUS_DATE, @CURRENT_STATUS, @WATER_DEPTH, " +
                "@SURFACE_LONGITUDE, @SURFACE_LATITUDE, @BOTTOM_HOLE_LONGITUDE, @BOTTOM_HOLE_LATITUDE)";
            await _da.SaveData<IEnumerable<GOMWellbore>>(connectionString, wellbores, sql);
        }

        public Task<IEnumerable<TableSchema>> GetColumnInfo(string connectionString, string table) =>
            _da.LoadData<TableSchema, dynamic>("dbo.sp_columns", new { TABLE_NAME = table }, connectionString);

        public async Task CopyDeviations(string connectionString)
        {
            try
            {
                var zipData = await _httpClient.GetByteArrayAsync(deviationsUrl);
                using var zipStream = new MemoryStream(zipData);
                using var archive = new ZipArchive(zipStream);

                _log.LogInformation("Data has been downloaded from GOM website");

                if (archive.Entries.Count == 0)
                {
                    _log.LogError("The zip archive is empty.");
                    return;
                }

                var entry = archive.GetEntry("directdelimit.txt");
                if (entry == null)
                {
                    _log.LogError("File not found in the zip archive.");
                    return;
                }

                _log.LogInformation("Start processing data");
                List<DeviationFile>  deviations = await ProcessDeviations(entry, connectionString);
                await SaveDeviationHeaders(deviations, connectionString);
                List<WellDirSrvyStation> devStations = ToWellDirSurveyStations(deviations);

                _log.LogInformation("Start saving deviation survey station data");
                string sql = "IF NOT EXISTS(SELECT 1 FROM WELL_DIR_SRVY_STATION WHERE UWI = @UWI AND DEPTH_OBS_NO = @DEPTH_OBS_NO) " +
                "INSERT INTO WELL_DIR_SRVY_STATION (UWI, SURVEY_ID, SOURCE, DEPTH_OBS_NO, AZIMUTH, INCLINATION, LATITUDE, LONGITUDE, STATION_MD) " +
                "VALUES(@UWI, @SURVEY_ID, @SOURCE, @DEPTH_OBS_NO, @AZIMUTH, @INCLINATION, @LATITUDE, @LONGITUDE, @STATION_MD)";
                foreach (var batch in devStations.Batch(3000))
                {
                    await _da.SaveData(connectionString, batch, sql);
                }

            }
            catch (HttpRequestException ex)
            {
                _log.LogError(ex, "Failed to download the ZIP file.");
            }
            catch (InvalidDataException ex)
            {
                _log.LogError(ex, "The downloaded file is not a valid ZIP archive.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An unexpected error occurred while processing deviations.");
            }

        }

        private async Task<List<DeviationFile>> ProcessDeviations(ZipArchiveEntry entry, string connectionString)
        {
            var deviations = new List<DeviationFile>();

            using var stream = entry.Open();
            using var reader = new StreamReader(stream);

            using var parser = new TextFieldParser(reader)
            {
                TextFieldType = FieldType.Delimited,
                Delimiters = new[] { "," },
                HasFieldsEnclosedInQuotes = true
            };

            while (!parser.EndOfData)
            {
                string[] fields = parser.ReadFields();

                if (fields.Length < 11)
                    continue; // Skip malformed lines

                deviations.Add(new DeviationFile
                {
                    ApiWellNumber = fields[0].Trim(),
                    Md = fields[1].GetDoubleFromString(),
                    DeviationAngle = fields[2].GetDoubleFromString(),
                    Azimuth = fields[3].GetDoubleFromString(),
                    Tvd = fields[4].GetDoubleFromString(),
                    Latitude = fields[5].GetDoubleFromString(),
                    Longitude = fields[6].GetDoubleFromString(),
                    DeclinationCorrection = fields[7].Trim(),
                    SurveyGridConvergence = fields[8].Trim(),
                    SurveyPointType = fields[9].Trim(),
                    LastUpdate = fields[10].GetDateFromString("MM/dd/yyyy")
                });
            }

            return deviations;
        }

        private async Task SaveDeviationHeaders(List<DeviationFile> deviations, string connectionString)
        {
            var grouped = deviations
                .GroupBy(d => d.ApiWellNumber)
                .Select(g => new
                {
                    ApiWellNumber = g.Key,
                    MaxMd = g.Max(x => x.Md) ?? 0.0,
                    MinMd = g.Min(x => x.Md) ?? 0.0
                })
                .ToList();

            List<WellDirSrvy> wellDirSurveys = new List<WellDirSrvy>();
            foreach (var header in grouped)
            {
                WellDirSrvy dirHeader = new WellDirSrvy
                {
                    UWI = header.ApiWellNumber,
                    SURVEY_ID = "1",
                    SOURCE = "BOEM",
                    AZIMUTH_NORTH_TYPE = "True North",
                    BASE_DEPTH = header.MaxMd,
                    TOP_DEPTH = header.MinMd,
                    COMPUTE_METHOD = "Minimum Curvature",
                    OFFSET_NORTH_TYPE = "True North"
                };
                wellDirSurveys.Add(dirHeader);
            }

            await SaveDeviationRefData(wellDirSurveys, connectionString);

            _log.LogInformation("Start saving deviation survey header data");
            string sql = "IF NOT EXISTS(SELECT 1 FROM WELL_DIR_SRVY WHERE UWI = @UWI) " +
                "INSERT INTO WELL_DIR_SRVY (UWI, SURVEY_ID, SOURCE, AZIMUTH_NORTH_TYPE, BASE_DEPTH, TOP_DEPTH, COMPUTE_METHOD, OFFSET_NORTH_TYPE) " +
                "VALUES(@UWI, @SURVEY_ID, @SOURCE, @AZIMUTH_NORTH_TYPE, @BASE_DEPTH, @TOP_DEPTH, @COMPUTE_METHOD, @OFFSET_NORTH_TYPE)";
            await _da.SaveData<IEnumerable<WellDirSrvy>>(connectionString, wellDirSurveys, sql);
        }

        public async Task SaveDeviationRefData(List<WellDirSrvy> deviations, string connectionString)
        {
            _log.LogInformation("Start saving deviation survey reference data");
            Dictionary<string, List<ReferenceData>> refDict = new Dictionary<string, List<ReferenceData>>();
            DeviationReferenceTables tables = new DeviationReferenceTables();

            List<ReferenceData> refs = deviations.Select(x => x.SOURCE).Distinct().ToList().CreateReferenceDataObject();
            refDict.Add(tables.RefTables[0].Table, refs);
            refs = deviations.Select(x => x.AZIMUTH_NORTH_TYPE).Distinct().ToList().CreateReferenceDataObject();
            refDict.Add(tables.RefTables[1].Table, refs);
            refs = deviations.Select(x => x.COMPUTE_METHOD).Distinct().ToList().CreateReferenceDataObject();
            refDict.Add(tables.RefTables[2].Table, refs);

            foreach (var table in tables.RefTables)
            {
                refs = refDict[table.Table];
                string sql = "";
                
                sql = $"IF NOT EXISTS(SELECT 1 FROM {table.Table} WHERE {table.KeyAttribute} = @Reference) " +
                    $"INSERT INTO {table.Table} " +
                    $"({table.KeyAttribute}, {table.ValueAttribute}) " +
                    $"VALUES(@Reference, @Reference)";
                await _da.SaveData(connectionString, refs, sql);
            }
        }

        private List<WellDirSrvyStation> ToWellDirSurveyStations(List<DeviationFile> deviations)
        {
            return deviations
                .Where(d => !string.IsNullOrWhiteSpace(d.ApiWellNumber))
                .Select(d => new WellDirSrvyStation
                {
                    UWI = d.ApiWellNumber,
                    SURVEY_ID = "1", // static since you only have one survey
                    SOURCE = "BOEM",
                    DEPTH_OBS_NO = (int)((d.Md ?? 0.0) * 1000),
                    AZIMUTH = d.Azimuth ?? 0.0,
                    INCLINATION = d.DeviationAngle ?? 0.0,
                    LATITUDE = d.Latitude ?? 0.0,
                    LONGITUDE = d.Longitude ?? 0.0,
                    STATION_MD = d.Md ?? 0.0
                })
                .ToList();
        }
    }
}
