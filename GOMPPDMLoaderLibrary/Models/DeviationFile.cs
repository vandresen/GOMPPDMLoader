using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOMPPDMLoaderLibrary.Models
{
    public class DeviationFile
    {
        public string ApiWellNumber { get; set; } = "";
        public double? Md { get; set; }
        public double? DeviationAngle { get; set; }
        public double? Azimuth { get; set; }
        public double? Tvd { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? DeclinationCorrection { get; set; }
        public string? SurveyGridConvergence { get; set; }
        public string? SurveyPointType { get; set; }
        public DateTime? LastUpdate { get; set; }
    }
}
