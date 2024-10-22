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
        //private readonly MessageStore messageStore;

        private string dbPath;

        private const string sql_insertCommand = "INSERT INTO messages (Timestamp, Temperature, Humidity, HeaterOn, IsHeaterOn) VALUES ($Timestamp, $Temperature, $Humidity, $HeaterOn, $IsHeaterOn);";

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

            if (GetDb().QuerySingle<int>("SELECT EXISTS (SELECT 1 FROM sqlite_master WHERE type='table' AND name='messages');") == 0)
            {
                string create = @"CREATE TABLE messages (Id INTEGER PRIMARY KEY, Timestamp TEXT, Temperature REAL, Humidity REAL, HeaterOn INTEGER, IsHeaterOn TEXT);";
                GetDb().Execute(create);
            }

            if (GetDb().QuerySingle<int>("SELECT EXISTS (SELECT 1 FROM sqlite_master WHERE type='table' AND name='models');") == 0)
            {
                string create = @"CREATE TABLE models (Id INTEGER PRIMARY KEY, Timestamp TEXT, RMSE REAL);";
                GetDb().Execute(create);
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
                cmd.CommandText = sql_insertCommand;
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

        public float[] GetModels()
        {
            using (var db = GetDb())
            {
                db.Open();
                var cmd = db.CreateCommand();
                cmd.CommandText = "SELECT RMSE FROM models ORDER BY Id DESC LIMIT 10;";
                var reader = cmd.ExecuteReader();
                var results = new List<float>();
                while (reader.Read())
                {
                    results.Add(reader.GetFloat(0));
                }
                db.Close();
                return results.ToArray();
            }
        }

        //public List<InputData> GetTrainDataset(int dimension)
        //{
        //    using (var db = GetDb())
        //    {
        //        db.Open();
        //        var cmd = db.CreateCommand();
        //        cmd.CommandText = TrainDatasetCommand;
        //        var reader = cmd.ExecuteReader();
        //        var results = new List<InputData>(dimension);
        //        while (reader.Read())
        //        {
        //            var id = new InputData();

        //            id.temperature = reader.CheckConvert<float>("Temperature");
        //            id.tempLag1 = reader.CheckConvert<float>("tempLag1");
        //            id.tempLag2 = reader.CheckConvert<float>("tempLag2");
        //            id.tempLag3 = reader.CheckConvert<float>("tempLag3");
        //            id.tempLag4 = reader.CheckConvert<float>("tempLag4");
        //            id.tempLag5 = reader.CheckConvert<float>("tempLag5");
        //            id.tempLag6 = reader.CheckConvert<float>("tempLag6");
        //            id.tempLag7 = reader.CheckConvert<float>("tempLag7");
        //            id.tempLag8 = reader.CheckConvert<float>("tempLag8");
        //            id.tempLag9 = reader.CheckConvert<float>("tempLag9");
        //            id.tempLag10 = reader.CheckConvert<float>("tempLag10");
        //            id.tempLag11 = reader.CheckConvert<float>("tempLag11");
        //            id.tempLag12 = reader.CheckConvert<float>("tempLag12");
        //            id.tempLag13 = reader.CheckConvert<float>("tempLag13");
        //            id.tempLag14 = reader.CheckConvert<float>("tempLag14");
        //            id.tempLag15 = reader.CheckConvert<float>("tempLag15");
        //            id.tempLag16 = reader.CheckConvert<float>("tempLag16");
        //            id.tempLag17 = reader.CheckConvert<float>("tempLag17");
        //            id.tempLag18 = reader.CheckConvert<float>("tempLag18");
        //            id.tempLag19 = reader.CheckConvert<float>("tempLag19");
        //            id.tempLag20 = reader.CheckConvert<float>("tempLag20");
        //            id.tempLag21 = reader.CheckConvert<float>("tempLag21");
        //            id.tempLag22 = reader.CheckConvert<float>("tempLag22");
        //            id.tempLag23 = reader.CheckConvert<float>("tempLag23");
        //            id.tempLag24 = reader.CheckConvert<float>("tempLag24");
        //            //id.humidity = reader.CheckConvert<float>("Humidity");
        //            results.Add(id);

        //        }
        //        db.Close();
        //        return results;
        //    }
        //}

        public int GetMessagesCount()
        {
            return GetDb().QuerySingle<int>("SELECT COUNT(*) FROM messages;");
        }

        public InputData GetPredictDataset(int lags)
        {
            return GetDb().QuerySingle<InputData>(PredictDatasetCommand(lags));
        }

        const string sql_LabelField = @" Temperature ";
        const string sql_staticFeatures = @" 
            STRFTIME(""%m"", ""Timestamp"") AS month, 
            STRFTIME(""%d"", ""Timestamp"") AS day, 
            STRFTIME(""%H"", ""Timestamp"") AS hour ";

        private string GenerateLags(int lags, string field, string shortName)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i <= lags; i++)
            {
                sb.Append($"LAG ({field}, {i}) OVER (ORDER BY Id) AS {shortName}Lag{i},");
            }
            sb.Length--;
            return sb.ToString();
        }

        public string TrainDatasetCommand(int lags) => @$"SELECT
            {sql_LabelField},
            {sql_staticFeatures},
            {GenerateLags(lags, "Temperature", "temp")}
            FROM messages;";
        
        private string PredictDatasetCommand(int lags) => @$"SELECT 
            {sql_staticFeatures},
            {GenerateLags(lags, "Temperature", "temp")} 
            FROM messages ORDER BY Id DESC LIMIT 1;";
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
