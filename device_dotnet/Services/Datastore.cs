using Iot.Device.DHTxx;
using Microsoft.Extensions.Logging;
using Nerdostat.Device.Models;
using Nerdostat.Shared;
using System;
using System.Device.Gpio;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnitsNet;
using LiteDB;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nerdostat.Device.Services
{
    public class Datastore
    {
        private readonly Configuration Config;
        private readonly ILogger<Datastore> log;

        private const string dbPath = "nerdostat.db";

        public Datastore(Configuration _config, ILogger<Datastore> _log)
        {
            Config = _config;
            log = _log;

            //if file in dbpath does not exist, create it
            if (!File.Exists(dbPath))
            {
                log.LogInformation("Creating new datastore");
                using (var db = new LiteDatabase(dbPath))
                {
                    var col = db.GetCollection<APIMessage>("messages");
                    col.EnsureIndex(x => x.Timestamp);
                }
            }
        }

        public void AddNew(APIMessage message)
        {
            using (var db = new LiteDatabase(dbPath))
            {
                var col = db.GetCollection<APIMessage>("messages");
                col.Insert(message);
                log.LogInformation("Added new message to datastore");
            }
        }

        //get all messages
        public List<APIMessage> GetMessages(int daysBack)
        {
            using (var db = new LiteDatabase(dbPath))
            {
                var col = db.GetCollection<APIMessage>("messages");
                var results = col.Find(Query.GTE("Timestamp", DateTime.Now.AddDays(-daysBack)));
                return results.ToList();
            }
        }

        //get sum of heater on time in the last x minutes
        public long GetHeaterOnTime(int minutes)
        {
            using (var db = new LiteDatabase(dbPath))
            {
                var col = db.GetCollection<APIMessage>("messages");
                var results = col.Find(Query.GTE("Timestamp", DateTime.Now.AddMinutes(-minutes)));
                return results.Select(m => m.HeaterOn).Sum() ?? 0;
            }
        }

        public float GetFirstTemperatureBefore(DateTime time)
        {
            using (var db = new LiteDatabase(dbPath))
            {
                var col = db.GetCollection<APIMessage>("messages");
                var results = col.Find(Query.LT("Timestamp", time)).OrderByDescending(m => m.Timestamp);
                var res = results.FirstOrDefault()?.Temperature ?? 0;
                return Convert.ToSingle(res);
            }
        }
    }
}
