using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectDetection
{
    internal class DetectionResult
    {
       
        public string Label { get; set; }

        public float Confidence { get; set; }

        internal DetectionResult(string Label, float Confidence)
        {
            this.Label = Label;
            this.Confidence = Confidence;
        }
    }
}
