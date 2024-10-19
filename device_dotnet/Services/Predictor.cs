using Microsoft.Extensions.Logging;
using Nerdostat.Shared;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnitsNet;
using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Trainers.FastTree;
using static Nerdostat.Device.Models.MLModels;
using Microsoft.ML.Data;
using Plotly.NET.LayoutObjects;
using Plotly.NET;
using System.Linq;
using System.Net.Http.Headers;
using System.Linq.Expressions;

namespace Nerdostat.Device.Services
{
    public class Predictor
    {
        private readonly Datastore Datastore;
        private readonly ILogger<Predictor> log;

        private const string modelPath = "nerdostat.model";

        public bool IsModelReady
        {
            get
            {
                return File.Exists(modelPath);
            }
        }

        public Predictor(ILogger<Predictor> _log, Datastore datastore)
        {
            Datastore = datastore;
            log = _log;
        }

        // try to predict the temperature in the next hour using mldotnet
        public void Train()
        {
            try
            {
                var inputData = new List<InputData>();

                var messages = Datastore.GetMessages(10);

                if (messages.Count < (60 / 5) * 24 * 10)
                {
                    log.LogInformation($"Not enough data to train model. Need at least 10 days of data.");
                    return;
                }

                log.LogInformation($"Getting data...");
                foreach (var message in messages)
                {
                    if (message.Temperature is null)
                    {
                        continue;
                    }

                    var id = new InputData
                    {
                        temperature = Convert.ToSingle(message.Temperature.Value),
                        humidity = Convert.ToSingle(message.Humidity.Value),
                        heaterStatus = Convert.ToSingle(message.IsHeaterOn),

                        heaterOnLast5Minutes = message.HeaterOn ?? 0,

                        tempLag1 = Datastore.GetFirstTemperatureBefore(message.Timestamp.AddMinutes(5 * 1)),
                        tempLag2 = Datastore.GetFirstTemperatureBefore(message.Timestamp.AddMinutes(5 * 2)),
                        tempLag3 = Datastore.GetFirstTemperatureBefore(message.Timestamp.AddMinutes(5 * 3)),
                        tempLag4 = Datastore.GetFirstTemperatureBefore(message.Timestamp.AddMinutes(5 * 4)),
                        tempLag5 = Datastore.GetFirstTemperatureBefore(message.Timestamp.AddMinutes(5 * 5)),
                        tempLag6 = Datastore.GetFirstTemperatureBefore(message.Timestamp.AddMinutes(5 * 6)),
                        tempLag7 = Datastore.GetFirstTemperatureBefore(message.Timestamp.AddMinutes(5 * 7)),
                        tempLag8 = Datastore.GetFirstTemperatureBefore(message.Timestamp.AddMinutes(5 * 8)),
                        tempLag9 = Datastore.GetFirstTemperatureBefore(message.Timestamp.AddMinutes(5 * 9)),
                        tempLag10 = Datastore.GetFirstTemperatureBefore(message.Timestamp.AddMinutes(5 * 10)),
                        tempLag11 = Datastore.GetFirstTemperatureBefore(message.Timestamp.AddMinutes(5 * 11)),
                        tempLag12 = Datastore.GetFirstTemperatureBefore(message.Timestamp.AddMinutes(5 * 12)),

                        heaterOnLast10Minutes = Datastore.GetHeaterOnTime(10),
                        heaterOnLast15Minutes = Datastore.GetHeaterOnTime(15),
                        heaterOnLast30Minutes = Datastore.GetHeaterOnTime(30),
                        heaterOnLastHour = Datastore.GetHeaterOnTime(60),
                        heaterOnLast2Hours = Datastore.GetHeaterOnTime(120),
                        heaterOnLast4Hours = Datastore.GetHeaterOnTime(240),
                        heaterOnLast8Hours = Datastore.GetHeaterOnTime(480),
                        heaterOnLast12Hours = Datastore.GetHeaterOnTime(720),
                        heaterOnLast24Hours = Datastore.GetHeaterOnTime(1440)
                    };

                    inputData.Add(id);
                }

                log.LogInformation($"Data loaded. Training model...");

                var mlContext = new MLContext();
                var data = mlContext.Data.LoadFromEnumerable(inputData);

                //var convertDataStep = mlContext.Transforms
                //    .Conversion.ConvertType(new[] {
                //        new InputOutputColumnPair(@"temperature_f", @"temperature"),
                //        new InputOutputColumnPair(@"humidity_f", @"humidity"),
                //        new InputOutputColumnPair(@"heaterStatus_f", @"heaterStatus"),
                //        new InputOutputColumnPair(@"heaterOnLast5Minutes_f", @"heaterOnLast5Minutes"),
                //        new InputOutputColumnPair(@"heaterOnLast10Minutes_f", @"heaterOnLast10Minutes"),
                //        new InputOutputColumnPair(@"heaterOnLast15Minutes_f", @"heaterOnLast15Minutes"),
                //        new InputOutputColumnPair(@"heaterOnLast30Minutes_f", @"heaterOnLast30Minutes"),
                //        new InputOutputColumnPair(@"heaterOnLastHour_f", @"heaterOnLastHour"),
                //        new InputOutputColumnPair(@"heaterOnLast2Hours_f", @"heaterOnLast2Hours"),
                //        new InputOutputColumnPair(@"heaterOnLast4Hours_f", @"heaterOnLast4Hours"),
                //        new InputOutputColumnPair(@"heaterOnLast8Hours_f", @"heaterOnLast8Hours"),
                //        new InputOutputColumnPair(@"heaterOnLast12Hours_f", @"heaterOnLast12Hours"),
                //        new InputOutputColumnPair(@"heaterOnLast24Hours_f", @"heaterOnLast24Hours")
                //        }, DataKind.Single);

                ////var transformer = convertDataStep.Fit(data);
                ////// Transforming the same data. This will add the 4 columns defined in
                ////// the pipeline, containing the converted
                ////// values of the initial columns. 
                ////var transformedData = transformer.Transform(data);

                //var predictStep = mlContext.Transforms
                //    .Concatenate(@"Features", new[] { @"humidity_f", @"heaterStatus_f", @"heaterOnLast5Minutes_f", @"heaterOnLast10Minutes_f", @"heaterOnLast15Minutes_f", @"heaterOnLast30Minutes_f", @"heaterOnLastHour_f", @"heaterOnLast2Hours_f", @"heaterOnLast4Hours_f", @"heaterOnLast8Hours_f", @"heaterOnLast12Hours_f", @"heaterOnLast24Hours_f" })
                //                    .Append(mlContext.Regression.Trainers.FastForest(new FastForestRegressionTrainer.Options() { LabelColumnName = @"temperature_f", FeatureColumnName = @"Features" }));
                //                    //.Append(mlContext.Regression.Trainers.FastForest(new FastForestRegressionTrainer.Options() { NumberOfTrees = 4, NumberOfLeaves = 4, FeatureFraction = 1F, LabelColumnName = @"temperature", FeatureColumnName = @"Features" }));

                var predictStep = mlContext.Transforms
                    .Concatenate(@"Features", new[] {
                    @"humidity",
                    @"heaterStatus",
                    @"tempLag1", @"tempLag2", @"tempLag3", @"tempLag4", @"tempLag5", @"tempLag6", @"tempLag7", @"tempLag8", @"tempLag9", @"tempLag10", @"tempLag11", @"tempLag12",
                    @"heaterOnLast5Minutes", @"heaterOnLast10Minutes", @"heaterOnLast15Minutes", @"heaterOnLast30Minutes", @"heaterOnLastHour", @"heaterOnLast2Hours", @"heaterOnLast4Hours", @"heaterOnLast8Hours", @"heaterOnLast12Hours", @"heaterOnLast24Hours" })
                                    .Append(mlContext.Regression.Trainers.FastForest(new FastForestRegressionTrainer.Options() { LabelColumnName = @"temperature", FeatureColumnName = @"Features" }));
                //.Append(mlContext.Regression.Trainers.FastForest(new FastForestRegressionTrainer.Options() { NumberOfTrees = 4, NumberOfLeaves = 4, FeatureFraction = 1F, LabelColumnName = @"temperature", FeatureColumnName = @"Features" }));


                var pipeline = predictStep;

                var model = pipeline.Fit(data);

                log.LogInformation($"Model trained. Saving model...");

                DataViewSchema dataViewSchema = data.Schema;
                using (var fs = File.Create(modelPath))
                {
                    mlContext.Model.Save(model, dataViewSchema, fs);
                }

                log.LogInformation($"Model saved. Plotting R2...");

                PlotRSquaredValues(data, model, "temperature");

                log.LogInformation($"R2 plotted.");
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
                log.LogInformation($"Predicting temperature...");

                var mlContext = new MLContext();
                ITransformer mlModel;

                log.LogInformation("Loading model...");
                using (var stream = File.OpenRead(modelPath))
                {
                    mlModel = mlContext.Model.Load(stream, out var _);
                }
                var predictionEngine = mlContext.Model.CreatePredictionEngine<InputData, OutputData>(mlModel);
                var input = new InputData
                {
                    humidity = Convert.ToSingle(message.Humidity.Value),
                    heaterStatus = Convert.ToSingle(message.IsHeaterOn),
                    heaterOnLast5Minutes = message.HeaterOn ?? 0,

                    tempLag1 = Datastore.GetFirstTemperatureBefore(message.Timestamp.AddMinutes(5 * 1)),
                    tempLag2 = Datastore.GetFirstTemperatureBefore(message.Timestamp.AddMinutes(5 * 2)),
                    tempLag3 = Datastore.GetFirstTemperatureBefore(message.Timestamp.AddMinutes(5 * 3)),
                    tempLag4 = Datastore.GetFirstTemperatureBefore(message.Timestamp.AddMinutes(5 * 4)),
                    tempLag5 = Datastore.GetFirstTemperatureBefore(message.Timestamp.AddMinutes(5 * 5)),
                    tempLag6 = Datastore.GetFirstTemperatureBefore(message.Timestamp.AddMinutes(5 * 6)),
                    tempLag7 = Datastore.GetFirstTemperatureBefore(message.Timestamp.AddMinutes(5 * 7)),
                    tempLag8 = Datastore.GetFirstTemperatureBefore(message.Timestamp.AddMinutes(5 * 8)),
                    tempLag9 = Datastore.GetFirstTemperatureBefore(message.Timestamp.AddMinutes(5 * 9)),
                    tempLag10 = Datastore.GetFirstTemperatureBefore(message.Timestamp.AddMinutes(5 * 10)),
                    tempLag11 = Datastore.GetFirstTemperatureBefore(message.Timestamp.AddMinutes(5 * 11)),
                    tempLag12 = Datastore.GetFirstTemperatureBefore(message.Timestamp.AddMinutes(5 * 12)),

                    heaterOnLast10Minutes = Datastore.GetHeaterOnTime(10),
                    heaterOnLast15Minutes = Datastore.GetHeaterOnTime(15),
                    heaterOnLast30Minutes = Datastore.GetHeaterOnTime(30),
                    heaterOnLastHour = Datastore.GetHeaterOnTime(60),
                    heaterOnLast2Hours = Datastore.GetHeaterOnTime(120),
                    heaterOnLast4Hours = Datastore.GetHeaterOnTime(240),
                    heaterOnLast8Hours = Datastore.GetHeaterOnTime(480),
                    heaterOnLast12Hours = Datastore.GetHeaterOnTime(720),
                    heaterOnLast24Hours = Datastore.GetHeaterOnTime(1440)
                };
                log.LogInformation("Predicting...");
                var prediction = predictionEngine.Predict(input);
                log.LogInformation($"Predicted temperature: {prediction.temperature}");
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
    }
}
