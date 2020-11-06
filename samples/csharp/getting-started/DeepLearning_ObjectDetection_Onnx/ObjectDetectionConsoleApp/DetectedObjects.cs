using System;
using System.Collections.Generic;
using System.Text;

namespace Algonia.ML
{
    internal class DetectedObjects
    {
        public string Label { get; set; }
        public float Confidence { get; set; }

        internal DetectedObjects(string Label, float Confidence)
        {
            this.Label = Label;
            this.Confidence = Confidence;
        }
    }
}
