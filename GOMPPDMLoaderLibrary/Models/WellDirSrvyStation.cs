using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOMPPDMLoaderLibrary.Models
{
    public class WellDirSrvyStation
    {
        public string UWI { get; set; }
        public string SURVEY_ID { get; set; }
        public string SOURCE { get; set; }
        public int DEPTH_OBS_NO { get; set; }
        public double AZIMUTH { get; set; }
        public double INCLINATION { get; set; }
        public double LATITUDE { get; set; }
        public double LONGITUDE { get; set; }
        public double STATION_MD { get; set; }
    }
}
