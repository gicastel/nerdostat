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

            public float month { get; set; }
            public float day { get; set; }
            public float hour { get; set; }

            //public float humidity { get; set; }
            //public float heaterStatus { get; set; }

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
            public float tempLag13 { get; set; }
            public float tempLag14 { get; set; }
            public float tempLag15 { get; set; }
            public float tempLag16 { get; set; }
            public float tempLag17 { get; set; }
            public float tempLag18 { get; set; }
            public float tempLag19 { get; set; }
            public float tempLag20 { get; set; }
            public float tempLag21 { get; set; }
            public float tempLag22 { get; set; }
            public float tempLag23 { get; set; }
            public float tempLag24 { get; set; }
            public float tempLag25 { get; set; } 
            public float tempLag26 { get; set; }
            public float tempLag27 { get; set; }
            public float tempLag28 { get; set; }
            public float tempLag29 { get; set; }
            public float tempLag30 { get; set; }
            public float tempLag31 { get; set; }
            public float tempLag32 { get; set; }
            public float tempLag33 { get; set; }
            public float tempLag34 { get; set; }
            public float tempLag35 { get; set; }
            public float tempLag36 { get; set; }
            public float tempLag37 { get; set; }
            public float tempLag38 { get; set; }
            public float tempLag39 { get; set; }
            public float tempLag40 { get; set; }
            public float tempLag41 { get; set; }
            public float tempLag42 { get; set; }
            public float tempLag43 { get; set; }
            public float tempLag44 { get; set; }
            public float tempLag45 { get; set; }
            public float tempLag46 { get; set; }
            public float tempLag47 { get; set; }
            public float tempLag48 { get; set; }
            public float tempLag49 { get; set; }
            public float tempLag50 { get; set; }
            public float tempLag51 { get; set; }
            public float tempLag52 { get; set; }
            public float tempLag53 { get; set; }
            public float tempLag54 { get; set; }
            public float tempLag55 { get; set; }
            public float tempLag56 { get; set; }
            public float tempLag57 { get; set; }
            public float tempLag58 { get; set; }
            public float tempLag59 { get; set; }
            public float tempLag60 { get; set; }
            public float tempLag61 { get; set; }
            public float tempLag62 { get; set; }
            public float tempLag63 { get; set; }
            public float tempLag64 { get; set; }
            public float tempLag65 { get; set; }
            public float tempLag66 { get; set; }
            public float tempLag67 { get; set; }
            public float tempLag68 { get; set; }
            public float tempLag69 { get; set; }
            public float tempLag70 { get; set; }
            public float tempLag71 { get; set; }
            public float tempLag72 { get; set; }



            //public float heaterOnLast5Minutes { get; set; }
            //public float heaterOnLast10Minutes { get; set; }
            //public float heaterOnLast15Minutes { get; set; }
            //public float heaterOnLast30Minutes { get; set; }
            //public float heaterOnLastHour { get; set; }
            //public float heaterOnLast2Hours { get; set; }
            //public float heaterOnLast4Hours { get; set; }
            //public float heaterOnLast8Hours { get; set; }
            //public float heaterOnLast12Hours { get; set; }
            //public float heaterOnLast24Hours { get; set; }

            public float currentTemp { get; set; }
            public float currentHumidity { get; set; }
            public float currentPrecipitation { get; set; }
            public float currentCloudCover { get; set; }
            public float back1HourTemp { get; set; }
            public float back1HourHumidity { get; set; }
            public float back1HourPrecipitation { get; set; }
            public float back1HourCloudCover { get; set; }
            public float back2HourTemp { get; set; }
            public float back2HourHumidity { get; set; }
            public float back2HourPrecipitation { get; set; }
            public float back2HourCloudCover { get; set; }
            public float back3HourTemp { get; set; }
            public float back3HourHumidity { get; set; }
            public float back3HourPrecipitation { get; set; }
            public float back3HourCloudCover { get; set; }
            public float back4HourTemp { get; set; }
            public float back4HourHumidity { get; set; }
            public float back4HourPrecipitation { get; set; }
            public float back4HourCloudCover { get; set; }
            public float back5HourTemp { get; set; }
            public float back5HourHumidity { get; set; }
            public float back5HourPrecipitation { get; set; }
            public float back5HourCloudCover { get; set; }
            public float back6HourTemp { get; set; }
            public float back6HourHumidity { get; set; }
            public float back6HourPrecipitation { get; set; }
            public float back6HourCloudCover { get; set; }
            public float nextHourTemp { get; set; }
            public float nextHourHumidity { get; set; }
            public float nextHourPrecipitation { get; set; }
            public float nextHourCloudCover { get; set; }
        }

        public class OutputData
        {
            [ColumnName("Score")]
            public float temperature { get; set; }
        }
    }
}
