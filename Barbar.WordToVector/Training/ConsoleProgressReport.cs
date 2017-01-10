using System;

namespace Barbar.WordToVector.Training
{
    public class ConsoleProgressReport : IProgressReport
    {
        private object m_SyncRoot = new object();

        public void OnVectorsTrainedProgress(double alpha, double progress, double persec)
        {
            lock (m_SyncRoot)
            {
                Console.Out.Write(string.Format("Alpha: {0:0.00000}  Progress: {1:0.00}  Words/thread/sec: {2}\r", alpha, progress, (int)persec));
                Console.Out.Flush();
            }
        }

        public void OnWordsTrainedFinished(int wordsTrained, int vocabularySize)
        {
            Console.Out.WriteLine();
            Console.Out.WriteLine($"Vocab size: {vocabularySize}");
            Console.Out.WriteLine($"Words in train file: {wordsTrained}");
        }

        public void OnWordsTrainedProgress(int wordsTrained)
        {
            Console.Out.Write($"Words trained: {wordsTrained / 1000}K\r");
            Console.Out.Flush();
        }
    }
}
