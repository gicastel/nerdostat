using Microsoft.Extensions.Logging;
using Nerdostat.Shared;
using System;
using System.IO;
using Microsoft.Data.Sqlite;
using static Nerdostat.Device.Models.MLModels;
using System.Text;
using Dapper;
using System.Collections.Generic;

namespace Nerdostat.Device.Services
{
    public class SqliteDatastore
    {
        private readonly ThermoConfiguration config;
        private readonly ILogger<SqliteDatastore> log;

        private string dbPath;

        public SqliteDatastore(ThermoConfiguration _config, ILogger<SqliteDatastore> _log)
        {
            config = _config;
            log = _log;

            dbPath = config.SqlDbPath;

            //if file in dbpath does not exist, create it
            if (!File.Exists(dbPath))
            {
                log.LogInformation("Creating new datastore sqlite");
                GetDb().Execute("SELECT 1");
            }

            // Leggi il file con la definizione del database ed eseguilo
            string dbSchemaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DatabaseSchema.sql");
            if (File.Exists(dbSchemaPath))
            {
                string dbSchema = File.ReadAllText(dbSchemaPath);
                GetDb().Execute(dbSchema);
            }
            else
            {
                log.LogError($"Database schema file not found at {dbSchemaPath}");
            }

            log.LogInformation("Checked datastore sqlite");            
        }
        private SqliteConnection GetDb() =>  new SqliteConnection($"Data Source = {config.SqlDbPath}");

        public void AddMessage(APIMessage message)
        {
            using (var db = GetDb())
            {
                db.Open();
                var cmd = db.CreateCommand();
                cmd.CommandText = "INSERT INTO messages (Timestamp, Temperature, Humidity, HeaterOn, IsHeaterOn) VALUES ($Timestamp, $Temperature, $Humidity, $HeaterOn, $IsHeaterOn);";
                cmd.Parameters.AddWithValue("$Timestamp", message.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                cmd.Parameters.AddWithValue("$Temperature", message.Temperature);
                cmd.Parameters.AddWithValue("$Humidity", message.Humidity);
                cmd.Parameters.AddWithValue("$HeaterOn", message.HeaterOn);
                cmd.Parameters.AddWithValue("$IsHeaterOn", message.IsHeaterOn.ToString());
                cmd.ExecuteNonQuery();
                db.Close();
                log.LogInformation("Added new message to datastore sqlite");
            }
        }

        public void AddModel(float rmse)
        {
            using (var db = GetDb())
            {
                db.Open();
                var cmd = db.CreateCommand();
                cmd.CommandText = "INSERT INTO models (Timestamp, RMSE) VALUES ($Timestamp, $RMSE);";
                cmd.Parameters.AddWithValue("$Timestamp", DateTime.Now);
                cmd.Parameters.AddWithValue("$RMSE", rmse);
                cmd.ExecuteNonQuery();
                db.Close();
                log.LogInformation("Added new model to datastore sqlite");
            }
        }

        public IEnumerable<float> GetModels()
        {
            var results = GetDb().Query<float>("SELECT RMSE FROM models ORDER BY Id DESC LIMIT 10;");
            return results;
        }

        public int GetMessagesCount()
        {
            return GetDb().QuerySingle<int>("SELECT COUNT(*) FROM messages;");
        }

        public InputData GetPredictDataset()
        {
            return GetDb().QuerySingle<InputData>("SELECT * FROM predictdata;");
        }

        private string GenerateLags(int lags, string field, string shortName, int startOffset = 1)
        {
            StringBuilder sb = new StringBuilder();
            int lagName = 1;
            for (int i = startOffset; i <= lags; i++, lagName++)
            {
                sb.Append($"LAG ({field}, {i}) OVER (ORDER BY msg.Id) AS {shortName}Lag{lagName},");
            }
            sb.Length--;
            return sb.ToString();
        }

        public string TrainDatasetCommand() => @$"SELECT * FROM traindata;";
        private string PredictDatasetCommand() => @$"SELECT * FROM predictdata;";
    
        public void UpsertMeteoData(DateTime dt, decimal temperature, decimal humidity, decimal precipitation, decimal cloudCover)
        {
            using (var db = GetDb())
            {
                db.Open();
                var cmd = db.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO meteodata (Timestamp, Temperature, Humidity, Precipitation, CloudCover) VALUES ($Timestamp, $Temperature, $Humidity, $Precipitation, $CloudCover)
                    ON CONFLICT (Timestamp) DO UPDATE SET Temperature = $Temperature, Humidity = $Humidity, Precipitation = $Precipitation, CloudCover = $CloudCover;";
                cmd.Parameters.AddWithValue("$Timestamp", dt.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                cmd.Parameters.AddWithValue("$Temperature", temperature);
                cmd.Parameters.AddWithValue("$Humidity", humidity);
                cmd.Parameters.AddWithValue("$Precipitation", precipitation);
                cmd.Parameters.AddWithValue("$CloudCover", cloudCover);
                cmd.ExecuteNonQuery();
                db.Close();
                log.LogInformation("Added new meteo data to datastore sqlite");
            }
        }

        public int GetMissingMeteoDataDays()
        {
            using (var db = GetDb())
            {
                // check if we have any forecast
                var lastForecast = db.QuerySingleOrDefault<DateTime>("SELECT Timestamp FROM meteodata ORDER BY Id DESC LIMIT 1;");
                
                if (lastForecast != default)
                    return (DateTime.Now - Convert.ToDateTime(lastForecast)).Days;

                var firstMessage = db.QuerySingle<DateTime>("SELECT Timestamp FROM messages ORDER BY Id LIMIT 1;");
                return (DateTime.Now - firstMessage).Days;
            }
        }
    }


    file static class ExtensionMethods
    {
        internal static T CheckConvert<T>(this SqliteDataReader reader, string fieldName)
        {
            if (reader.IsDBNull(0))
            {
                return default(T);
            }
            else
            {
                return reader.GetFieldValue<T>(0);
            }
        }
    }
}
