﻿using Common;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using System;
using System.Collections.Generic;
using System.IO;

namespace ProductRecommender
{
    class Program
    {
        //1. Do remember to replace amazon0302.txt with dataset from https://snap.stanford.edu/data/amazon0302.html
        //2. Replace column names with ProductID and CoPurchaseProductID. It should look like this:
        //   ProductID	ProductID_Copurchased
        //   0	1
        //   0  2
        private static string TrainingDataLocation = $"./Data/Amazon0302.txt";

        private static string ModelPath = $"./Model/model.zip";

        static void Main(string[] args)
        {
            //STEP 1: Create MLContext to be shared across the model creation workflow objects 
            MLContext mlContext = new MLContext();

            //STEP 2: Read the trained data using TextLoader by defining the schema for reading the product co-purchase dataset
            //        Do remember to replace amazon0302.txt with dataset from https://snap.stanford.edu/data/amazon0302.html
            var traindata = mlContext.Data.ReadFromTextFile(path:TrainingDataLocation,
                                                      columns: new[]
                                                                {
                                                                    new TextLoader.Column(DefaultColumnNames.Label, DataKind.R4, 0),
                                                                    new TextLoader.Column(name:nameof(ProductEntry.ProductID), type:DataKind.U4, source: new [] { new TextLoader.Range(0) }, keyCount: new KeyCount(262111)), 
                                                                    new TextLoader.Column(name:nameof(ProductEntry.CoPurchaseProductID), type:DataKind.U4, source: new [] { new TextLoader.Range(1) }, keyCount: new KeyCount(262111))
                                                                },
                                                      hasHeader: true,
                                                      separatorChar: '\t');

            //STEP 3: Your data is already encoded so all you need to do is specify options for MatrxiFactorizationTrainer with a few extra hyperparameters
            //        LossFunction, Alpa, Lambda and a few others like K and C as shown below and call the trainer. 
            MatrixFactorizationTrainer.Options options = new MatrixFactorizationTrainer.Options();
            options.MatrixColumnIndexColumnName = nameof(ProductEntry.ProductID);
            options.MatrixRowIndexColumnName = nameof(ProductEntry.CoPurchaseProductID);
            options.LabelColumnName= DefaultColumnNames.Label;
            options.LossFunction = MatrixFactorizationTrainer.LossFunctionType.SquareLossOneClass;
            options.Alpha = 0.01;
            options.Lambda = 0.025;
            // For better results use the following parameters
            //options.K = 100;
            //options.C = 0.00001;

            //Step 4: Call the MatrixFactorization trainer by passing options.
            var est = mlContext.Recommendation().Trainers.MatrixFactorization(options);
            
            //STEP 5: Train the model fitting to the DataSet
            //Please add Amazon0302.txt dataset from https://snap.stanford.edu/data/amazon0302.html to Data folder if FileNotFoundException is thrown.
            ITransformer model = est.Fit(traindata);

            //STEP 6: Create prediction engine and predict the score for Product 63 being co-purchased with Product 3.
            //        The higher the score the higher the probability for this particular productID being co-purchased 
            var predictionengine = model.CreatePredictionEngine<ProductEntry, Copurchase_prediction>(mlContext);
            var prediction = predictionengine.Predict(
                new ProductEntry()
                {
                    ProductID = 3,
                    CoPurchaseProductID = 63
                });

            Console.WriteLine("\n For ProductID = 3 and  CoPurchaseProductID = 63 the predicted score is " + Math.Round(prediction.Score, 1));
            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }

        public class Copurchase_prediction
        {
            public float Score { get; set; }
        }

        public class ProductEntry
        {
            [KeyType(Count = 262111)]
            public uint ProductID { get; set; }

            [KeyType(Count = 262111)]
            public uint CoPurchaseProductID { get; set; }
        }
    }
}
