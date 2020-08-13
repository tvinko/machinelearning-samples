using ObjectDetection;
using System;

namespace ObjectDetectionTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Wrapper objectDetection = new Wrapper("assets");
            objectDetection.Recognize();
        }
    }
}
