using CsvHelper.Configuration.Attributes;

namespace GOMPPDMLoaderLibrary.Models
{
    public class Company
    {
        [Index(0)]
        public string Number { get; set; }
        [Index(1)]
        public DateTime? StartDate { get; set; }
        public string CompanyName { get; set; }
    }
}
