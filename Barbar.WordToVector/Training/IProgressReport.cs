namespace Barbar.WordToVector.Training
{
    public interface IProgressReport
    {
        void OnWordsTrainedProgress(int wordsTrained);

        void OnWordsTrainedFinished(int wordsTrained, int vocabularySize);

        void OnVectorsTrainedProgress(double alpha, double progress, double persec);
    }
}
