using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nerdostat.Device.Models
{
    public class DbModels
    {
        public class DbMessage
        {
            public int Id { get; set; }
            public DateTime Timestamp { get; set; }
            public float Temperature { get; set; }
            public float Humidity { get; set; }
            public int HeaterOn { get; set; }
            public bool IsHeaterOn { get; set; }
        }

        public class DbMeteoData
        {
            public DateTime Timestamp { get; set; }
            public float Temperature { get; set; }
            public float Humidity { get; set; }
            public float Precipitation { get; set; }
            public float CloudCover { get; set; }
        }

    }
}
