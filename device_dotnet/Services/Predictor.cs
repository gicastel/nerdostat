using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Trainers.FastTree;
using static Nerdostat.Device.Models.MLModels;
using Microsoft.ML.Data;
using Microsoft.Data.Sqlite;
using System.Threading;
using System.Threading.Tasks;

namespace Nerdostat.Device.Services
{
    public class Predictor
    {
        private readonly ThermoConfiguration config;
        private readonly SqliteDatastore sqlStore;
        private readonly ILogger<Predictor> log;
        private readonly SqliteFactory sqliteFactory;

        private static SemaphoreSlim modelLock;

        private string modelPath;

        public Predictor(ILogger<Predictor> _log, SqliteDatastore _sqlStore, ThermoConfiguration _config)
        {
            sqlStore = _sqlStore;
            sqliteFactory = SqliteFactory.Instance;
            config = _config;
            log = _log;

            modelPath = config.ModelPath;
            
            modelLock = new SemaphoreSlim(1, 1);
        }

        // try to predict the temperature in the next hour using mldotnet
        public void Train(CancellationToken token)
        {
            try
            {
                modelLock.Wait();

                var msgCount = sqlStore.GetMessagesCount();
                if (msgCount < 72)
                {
                    log.LogInformation("Not enough data to train model. Need at least 72 messages, got {count}", msgCount);
                    return;
                }

                log.LogInformation("Training model with {count} messages", msgCount);

                var featuresColumns = GenerateFeatures();

                log.LogInformation("Getting data...");

                var mlContext = new MLContext();

                var dbloader = mlContext.Data.CreateDatabaseLoader<InputData>();

                DatabaseSource dbSource = new DatabaseSource(sqliteFactory, $"Data Source = {config.SqlDbPath}", sqlStore.TrainDatasetCommand());

                var data = dbloader.Load(dbSource);

                string[] datasetColumns = ["temperature", .. featuresColumns];

                log.LogInformation("Data loaded. Training model...");

                var notNulls = mlContext.Data.FilterRowsByMissingValues(data, datasetColumns);

                var predictStep = mlContext.Transforms
                    .Concatenate(@"Features", featuresColumns)
                    .AppendCacheCheckpoint(mlContext)
                    .Append(mlContext.Regression.Trainers.FastForest(
                        new FastForestRegressionTrainer.Options()
                        {
                            LabelColumnName = @"temperature",
                            FeatureColumnName = @"Features",
                            //NumberOfThreads = 3,
                            //AllowEmptyTrees = true,
                            //NumberOfLeaves = 4,
                            //NumberOfTrees = 4,
                            //FeatureFraction = 1F
                        }
                        ));
                //.Append(mlContext.Regression.Trainers.FastForest(new FastForestRegressionTrainer.Options() { NumberOfTrees = 4, NumberOfLeaves = 4, FeatureFraction = 1F, LabelColumnName = @"temperature", FeatureColumnName = @"Features" }));

                var pipeline = predictStep;

                var model = pipeline.Fit(data);

                log.LogInformation("Model trained. Saving model...");

                DataViewSchema dataViewSchema = data.Schema;
                using (var fs = File.Create(modelPath))
                {
                    mlContext.Model.Save(model, dataViewSchema, fs);
                }
                modelLock.Release();

                log.LogInformation("Model saved and ready. Evaluating model...");

                var predictions = model.Transform(data);

                IDataView testDataPredictions = model.Transform(data);
                RegressionMetrics trainedModelMetrics = mlContext.Regression.Evaluate(testDataPredictions, "temperature");

                sqlStore.AddModel(Convert.ToSingle(trainedModelMetrics.RootMeanSquaredError));
                //log last 10 rmses
                var rmses = sqlStore.GetModels();
                log.LogInformation("MAE: {mae}", trainedModelMetrics.MeanAbsoluteError);
                log.LogInformation("Last 10 RMSEs: {rmses}", string.Join(" - ", rmses));
            }
            catch (Exception ex)
            {
                modelLock.Release();
                log.LogError(ex, "Error training model");
            }
        }

        public async Task<double?> Predict(CancellationToken token)
        {
            try
            {
                if (!await modelLock.WaitAsync(0, token))
                {
                    log.LogInformation("Model is not ready yet");
                    return null;
                }

                log.LogInformation("Predicting temperature...");

                var mlContext = new MLContext();
                ITransformer mlModel;

                log.LogInformation("Loading model...");
                using (var stream = File.OpenRead(modelPath))
                {
                    mlModel = mlContext.Model.Load(stream, out var _);
                }
                var predictionEngine = mlContext.Model.CreatePredictionEngine<InputData, OutputData>(mlModel);

                // Load Trained Model
                //log.LogInformation("Loading model V2...");
                //DataViewSchema predictionPipelineSchema;
                //ITransformer predictionPipeline = mlContext.Model.Load(modelPath, out predictionPipelineSchema);
                // Create PredictionEngines
                //PredictionEngine<InputData, OutputData> predictionEngine = mlContext.Model.CreatePredictionEngine<InputData, OutputData>(predictionPipeline);

                var input = sqlStore.GetPredictDataset();
                
                log.LogInformation("Predicting...");
                var prediction = predictionEngine.Predict(input);
                modelLock.Release();
                log.LogInformation("Predicted temperature: {pred}", prediction.temperature);
                return prediction.temperature;
            }
            catch (Exception ex)
            {
                modelLock.Release();
                log.LogError(ex, "Error predicting temperature");
                return null;
            }
        }

        private string[] GenerateFeatures()
        {
            List<string> features = new List<string>(107)
            {
                //add static features here
                "month",
                "day",
                "hour"
            };

            for (int i = 1; i <= 73; i++)
            {
                features.Add($"tempLag{i}");
            }

            features.Add("currentTemp");
            features.Add("currentHumidity");
            features.Add("currentPrecipitation");
            features.Add("currentCloudCover");
            features.Add("back1HourTemp");
            features.Add("back1HourHumidity");
            features.Add("back1HourPrecipitation");
            features.Add("back1HourCloudCover");
            features.Add("back2HourTemp");
            features.Add("back2HourHumidity");
            features.Add("back2HourPrecipitation");
            features.Add("back2HourCloudCover");
            features.Add("back3HourTemp");
            features.Add("back3HourHumidity");
            features.Add("back3HourPrecipitation");
            features.Add("back3HourCloudCover");
            features.Add("back4HourTemp");
            features.Add("back4HourHumidity");
            features.Add("back4HourPrecipitation");
            features.Add("back4HourCloudCover");
            features.Add("back5HourTemp");
            features.Add("back5HourHumidity");
            features.Add("back5HourPrecipitation");
            features.Add("back5HourCloudCover");
            features.Add("back6HourTemp");
            features.Add("back6HourHumidity");
            features.Add("back6HourPrecipitation");
            features.Add("back6HourCloudCover");
            features.Add("nextHourTemp");
            features.Add("nextHourHumidity");
            features.Add("nextHourPrecipitation");
            features.Add("nextHourCloudCover");

            return features.ToArray();
        }
    }
}
