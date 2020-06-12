using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace UnitTest1
{
    [TestClass]
    public class UnitTest1
    {
        [DataTestMethod]
        [DataRow(@"BinaryClassification_SentimentAnalysis\SentimentAnalysis\SentimentAnalysisConsoleApp")]
        [DataRow(@"BinaryClassification_HeartDiseaseDetection\HeartDiseaseDetection\HeartDiseaseDetection")]
        [DataRow(@"BinaryClassification_CreditCardFraudDetection\CreditCardFraudDetection.Trainer")]
        public void TestMethod1(string projectPath)
        {
            using (Process process1 = new Process())
            {
                process1.StartInfo.FileName = "dotnet";
                process1.StartInfo.Arguments = "run";
                process1.StartInfo.RedirectStandardOutput = true;

                var basePath = @"C:\Users\brachtma\Documents\GitHub\machinelearning-samples\samples\csharp\getting-started";

                // Note: console writeline isn't output to console unless "dotnet run" is run directly at root of csproj
                string path = process1.StartInfo.WorkingDirectory;
                //process1.StartInfo.WorkingDirectory = Path.GetFullPath(Path.Combine(path, @"..\..\..\..\..\BinaryClassification_SentimentAnalysis\SentimentAnalysis\SentimentAnalysisConsoleApp"));
                process1.StartInfo.WorkingDirectory = Path.Combine(basePath, projectPath);

                //process1.StartInfo.WorkingDirectory = @"C:\Users\brachtma\Documents\GitHub\machinelearning-samples\samples\csharp\getting-started\BinaryClassification_SentimentAnalysis\SentimentAnalysis\SentimentAnalysisConsoleApp";

                process1.StartInfo.UseShellExecute = false;
                process1.Start();

                // Since the program waits for a user key to end after printing the out,
                // wait 30 seconds to give it time to output before manually killing the app.
                if (!process1.WaitForExit(30 * 1000))
                {
                    process1.Kill();
                }

                var appOutput = process1.StandardOutput.ReadToEnd();

                // Warning: due to the "WaitForExit" the output can be very flaky, even waiting 30 seconds.
                // This may change depending on machine.
                // Check the output of any failed test to see what went wrong.
                if (appOutput == "")
                {
                    Console.WriteLine("App failed to write output: " + appOutput);
                }
                Console.WriteLine(appOutput);

                // Assert that the last line of output printed
                Assert.IsTrue(appOutput.Contains("End of process. Hit any key to exit"));
            }
        }
    }
}
