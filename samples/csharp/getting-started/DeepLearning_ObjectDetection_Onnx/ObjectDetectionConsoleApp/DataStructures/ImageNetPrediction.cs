using Microsoft.ML.Data;

namespace Algonia.ML.DataStructures
{
    public class ImageNetPrediction
    {
        [ColumnName("grid")]
        public float[] PredictedLabels;
    }
}
