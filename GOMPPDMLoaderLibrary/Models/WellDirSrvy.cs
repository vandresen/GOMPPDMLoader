using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOMPPDMLoaderLibrary.Models
{
    public class WellDirSrvy
    {
        public string UWI { get; set; }
        public string SURVEY_ID { get; set; }
        public string SOURCE { get; set; }
        public string AZIMUTH_NORTH_TYPE { get; set; }
        public double BASE_DEPTH { get; set; }
        public string COMPUTE_METHOD { get; set; }
        public string OFFSET_NORTH_TYPE { get; set; }
        public double TOP_DEPTH { get; set; }
    }
}
