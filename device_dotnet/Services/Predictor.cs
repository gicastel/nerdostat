using Microsoft.Extensions.Logging;
using Nerdostat.Shared;
using System;
using System.IO;
using UnitsNet;
using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Trainers.FastTree;
using static Nerdostat.Device.Models.MLModels;
using Microsoft.ML.Data;
using Plotly.NET.LayoutObjects;
using Plotly.NET;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.ML.Trainers;
using System.Collections;

namespace Nerdostat.Device.Services
{
    public class Predictor
    {
        private readonly ThermoConfiguration config;
        private readonly SqliteDatastore sqlStore;
        private readonly ILogger<Predictor> log;
        private readonly SqliteFactory sqliteFactory;

        private string modelPath;

        private bool isModelReady = false;

        private int lagData;

        public bool IsModelReady
        {
            get
            {
                return isModelReady;
            }
        }

        public Predictor(ILogger<Predictor> _log, SqliteDatastore _sqlStore, ThermoConfiguration _config)
        {
            sqlStore = _sqlStore;
            sqliteFactory = SqliteFactory.Instance;
            config = _config;
            log = _log;

            lagData = 24;
            modelPath = config.ModelPath;

            if (File.Exists(modelPath))
            {
                isModelReady = true;
            }
        }

        // try to predict the temperature in the next hour using mldotnet
        public void Train()
        {
            try
            {
                isModelReady = false;

                var msgCount = sqlStore.GetMessagesCount();
                if (msgCount < lagData)
                {
                    log.LogInformation("Not enough data to train model. Need at least 24 messages, got {count}", msgCount);
                    return;
                }

                // go back at max 2 days
                if (msgCount > 48 * 12)
                    msgCount = 48 * 12;

                lagData = msgCount;

                log.LogInformation("Training model with {count} hours", msgCount / 12);

                var featuresColumns = GenerateFeatures();

                log.LogInformation("Getting data...");

                var mlContext = new MLContext();

                var dbloader = mlContext.Data.CreateDatabaseLoader<InputData>();

                DatabaseSource dbSource = new DatabaseSource(sqliteFactory, $"Data Source = {config.SqlDbPath}", sqlStore.TrainDatasetCommand(lagData));

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
                isModelReady = true;

                log.LogInformation("Model saved and ready. Evaluating model...");

                var predictions = model.Transform(data);

                IDataView testDataPredictions = model.Transform(data);
                RegressionMetrics trainedModelMetrics = mlContext.Regression.Evaluate(testDataPredictions, "temperature");

                sqlStore.AddModel(Convert.ToSingle(trainedModelMetrics.RootMeanSquaredError));
                //log last 10 rmses
                var rmses = sqlStore.GetModels();
                log.LogInformation("MAE: {mae}", trainedModelMetrics.MeanAbsoluteError);
                log.LogInformation("Last 10 RMSEs: {rmses}", string.Join(" - ", rmses));

                //PlotRSquaredValues(data, model, "temperature");
                //log.LogInformation("R2 plotted.");

            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error training model");
            }
        }

        public double Predict(APIMessage message)
        {
            try
            {
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

                var input = sqlStore.GetPredictDataset(lagData);
                log.LogInformation("Predicting...");
                var prediction = predictionEngine.Predict(input);
                log.LogInformation("Predicted temperature: {pred}", prediction.temperature);
                return prediction.temperature;
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error predicting temperature");
                return 0;
            }
        }

        private void PlotRSquaredValues(IDataView trainData, ITransformer model, string labelColumnName)
        {
            // Number of rows to display in charts.
            int numberOfRows = 1000;
            // Use the model to make batch predictions on training data
            var testResults = model.Transform(trainData);

            // Get the actual values from the dataset
            var trueValues = testResults.GetColumn<float>(labelColumnName).Take(numberOfRows); ;

            // Get the predicted values from the test results
            var predictedValues = testResults.GetColumn<float>("Score").Take(numberOfRows);

            // Setup what the graph looks like
            var title = Title.init(Text: "R-Squared Plot");
            var layout = Layout.init<IConvertible>(Title: title, PlotBGColor: Plotly.NET.Color.fromString("#e5ecf6"));
            var xAxis = LinearAxis.init<IConvertible, IConvertible, IConvertible, IConvertible, IConvertible, IConvertible>(
                    Title: Title.init("True Values"),
                    ZeroLineColor: Plotly.NET.Color.fromString("#ffff"),
                    GridColor: Plotly.NET.Color.fromString("#ffff"),
                    ZeroLineWidth: 2);
            var yAxis = LinearAxis.init<IConvertible, IConvertible, IConvertible, IConvertible, IConvertible, IConvertible>(
                    Title: Title.init("Predicted Values"),
                    ZeroLineColor: Plotly.NET.Color.fromString("#ffff"),
                    GridColor: Plotly.NET.Color.fromString("#ffff"),
                    ZeroLineWidth: 2);

            // We will plot the line that shows the perfect result. Setup that line here.
            var maximumValue = Math.Max(trueValues.Max(), predictedValues.Max());
            var perfectX = new[] { 0, maximumValue };
            var perfectY = new[] { 0, maximumValue };



            // Create the scatterplot that shows the true values vs the predicted values
            var trueAndPredictedValues = Chart2D.Chart.Scatter<float, float, string>(x: trueValues, y: predictedValues, mode: StyleParam.Mode.Markers)
                            .WithLayout(layout)
                            .WithXAxis(xAxis)
                            .WithYAxis(yAxis);

            // Setup the line that shows what a perfect prediction would look like
            var perfectLineGraph = Chart2D.Chart.Line<float, float, string>(x: perfectX, y: perfectY)
                            .WithLayout(layout)
                            .WithLine(Line.init(Width: 1.5));

            var chartWithValuesAndIdealLine = Chart.Combine(new[] { trueAndPredictedValues, perfectLineGraph });
            var chartFilePath = "RegressionChart.html";

            chartWithValuesAndIdealLine.SaveHtml(chartFilePath);
        }

        private string[] GenerateFeatures()
        {
            List<string> features = new List<string>(lagData+3);

            //add static features here
            features.Add("month");
            features.Add("day");
            features.Add("hour");

            for (int i = 1; i <= lagData; i++)
            {
                features.Add($"tempLag{i}");
            }
            return features.ToArray();
        }
    }
}
