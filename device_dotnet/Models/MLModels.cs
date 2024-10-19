using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nerdostat.Device.Models
{
    public class MLModels
    {
        public class InputData
        {
            public float temperature { get; set; }
            public float humidity { get; set; }
            public float heaterStatus { get; set; }

            public float tempLag1 { get; set; }
            public float tempLag2 { get; set; }
            public float tempLag3 { get; set; }
            public float tempLag4 { get; set; }
            public float tempLag5 { get; set; }
            public float tempLag6 { get; set; }
            public float tempLag7 { get; set; }
            public float tempLag8 { get; set; }
            public float tempLag9 { get; set; }
            public float tempLag10 { get; set; }
            public float tempLag11 { get; set; }
            public float tempLag12 { get; set; }

            public float heaterOnLast5Minutes { get; set; }
            public float heaterOnLast10Minutes { get; set; }
            public float heaterOnLast15Minutes { get; set; }
            public float heaterOnLast30Minutes { get; set; }
            public float heaterOnLastHour { get; set; }
            public float heaterOnLast2Hours { get; set; }
            public float heaterOnLast4Hours { get; set; }
            public float heaterOnLast8Hours { get; set; }
            public float heaterOnLast12Hours { get; set; }
            public float heaterOnLast24Hours { get; set; }
        }

        public class OutputData
        {
            [ColumnName("Score")]
            public float temperature { get; set; }
        }
    }
}
