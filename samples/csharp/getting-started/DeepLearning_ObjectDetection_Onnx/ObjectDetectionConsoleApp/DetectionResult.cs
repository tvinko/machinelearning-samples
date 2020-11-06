using System;
using System.Collections.Generic;
using System.Text;

namespace Algonia.ML
{
    internal class DetectionResult
    {
        public string FileName { get; set; }
        public List<DetectedObjects> DetectedObjects { get; set; }

        public DetectionResult(string FileName)
        {
            this.FileName = FileName;
            this.DetectedObjects = new List<DetectedObjects>();
        }
    }
}
