﻿using System;
using Microsoft.ML;
using System.IO;
using BikeSharingDemand.DataStructures;
using Common;
using Microsoft.ML.Data;
using System.Reflection;

namespace BikeSharingDemand
{
    public class Wrapper
    {
        string TrainingDataLocation { get; set; }
        string TestDataLocation { get; set; }
        string ModelsLocation { get; set; }

        public Wrapper(string TrainingDataLocation, string TestDataLocation, string ModelsLocation)
        {
            this.TrainingDataLocation = Path.Combine(Environment.CurrentDirectory, TrainingDataLocation);
            this.TestDataLocation = Path.Combine(Environment.CurrentDirectory, TestDataLocation);
            this.ModelsLocation = Path.Combine(Environment.CurrentDirectory, ModelsLocation);
        }

        public void Start()
        {
            Console.WriteLine("Training data location: " + TrainingDataLocation);
            Console.WriteLine("Test data location: " + TestDataLocation);
            Console.WriteLine("Models location: " + ModelsLocation);
            // Create MLContext to be shared across the model creation workflow objects 
            // Set a random seed for repeatable/deterministic results across multiple trainings.
            var mlContext = new MLContext(seed: 0);

            // 1. Common data loading configuration
            var trainingDataView = mlContext.Data.LoadFromTextFile<DemandObservation>(path: TrainingDataLocation, hasHeader: true, separatorChar: ',');
            var testDataView = mlContext.Data.LoadFromTextFile<DemandObservation>(path: TestDataLocation, hasHeader: true, separatorChar: ',');

            // 2. Common data pre-process with pipeline data transformations

            // Concatenate all the numeric columns into a single features column
            var dataProcessPipeline = mlContext.Transforms.Concatenate("Features",
                                                     nameof(DemandObservation.Season), nameof(DemandObservation.Year), nameof(DemandObservation.Month),
                                                     nameof(DemandObservation.Hour), nameof(DemandObservation.Holiday), nameof(DemandObservation.Weekday),
                                                     nameof(DemandObservation.WorkingDay), nameof(DemandObservation.Weather), nameof(DemandObservation.Temperature),
                                                     nameof(DemandObservation.NormalizedTemperature), nameof(DemandObservation.Humidity), nameof(DemandObservation.Windspeed))
                                         .AppendCacheCheckpoint(mlContext);
            // Use in-memory cache for small/medium datasets to lower training time. 
            // Do NOT use it (remove .AppendCacheCheckpoint()) when handling very large datasets.

            // (Optional) Peek data in training DataView after applying the ProcessPipeline's transformations  
            Common.ConsoleHelper.PeekDataViewInConsole(mlContext, trainingDataView, dataProcessPipeline, 10);
            Common.ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", trainingDataView, dataProcessPipeline, 10);

            // Definition of regression trainers/algorithms to use
            //var regressionLearners = new (string name, IEstimator<ITransformer> value)[]
            (string name, IEstimator<ITransformer> value)[] regressionLearners =
            {
                ("FastTree", mlContext.Regression.Trainers.FastTree()),
                ("Poisson", mlContext.Regression.Trainers.LbfgsPoissonRegression()),
                ("SDCA", mlContext.Regression.Trainers.Sdca()),
                ("FastTreeTweedie", mlContext.Regression.Trainers.FastTreeTweedie()),
                //Other possible learners that could be included
                //...FastForestRegressor...
                //...GeneralizedAdditiveModelRegressor...
                //...OnlineGradientDescent... (Might need to normalize the features first)
            };

            // 3. Phase for Training, Evaluation and model file persistence
            // Per each regression trainer: Train, Evaluate, and Save a different model
            foreach (var trainer in regressionLearners)
            {
                Console.WriteLine("=============== Training the current model ===============");
                var trainingPipeline = dataProcessPipeline.Append(trainer.value);
                var trainedModel = trainingPipeline.Fit(trainingDataView);

                Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
                IDataView predictions = trainedModel.Transform(testDataView);
                var metrics = mlContext.Regression.Evaluate(data: predictions, labelColumnName: "Label", scoreColumnName: "Score");
                ConsoleHelper.PrintRegressionMetrics(trainer.value.ToString(), metrics);

                //Save the model file that can be used by any application
                mlContext.Model.Save(trainedModel, trainingDataView.Schema, $"{ModelsLocation}/{trainer.name}Model.zip");
                Console.WriteLine("The model is saved to {0}", $"{ModelsLocation}/{trainer.name}Model.zip");
            }

            // 4. Try/test Predictions with the created models
            // The following test predictions could be implemented/deployed in a different application (production apps)
            // that's why it is seggregated from the previous loop
            // For each trained model, test 10 predictions           
            foreach (var learner in regressionLearners)
            {
                //Load current model from .ZIP file
                string modelPath = $"{ModelsLocation}/{learner.name}Model.zip";
                ITransformer trainedModel = mlContext.Model.Load(modelPath, out var modelInputSchema);

                // Create prediction engine related to the loaded trained model
                var predEngine = mlContext.Model.CreatePredictionEngine<DemandObservation, DemandPrediction>(trainedModel);

                Console.WriteLine($"================== Visualize/test 10 predictions for model {learner.name}Model.zip ==================");
                //Visualize 10 tests comparing prediction with actual/observed values from the test dataset
                ModelScoringTester.VisualizeSomePredictions(mlContext, learner.name, TestDataLocation, predEngine, 10);
            }
        }
    }
}
