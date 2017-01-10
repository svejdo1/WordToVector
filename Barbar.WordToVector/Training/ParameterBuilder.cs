namespace Barbar.WordToVector.Training
{
    public class ParameterBuilder : IParameterBuilder
    {
        private Parameter _parameter = new Parameter();
        private float? _alpha;

        public ParameterBuilder SetVectorSize(int value)
        {
            _parameter.VectorSize = value;
            return this;
        }

        public ParameterBuilder SetTrainFile(string value)
        {
            _parameter.TrainFile = value;
            return this;
        }

        public ParameterBuilder SetVocabularyFileToRead(string value)
        {
            _parameter.VocabularyReadFile = value;
            return this;
        }

        public ParameterBuilder SetVocabularyFileToWrite(string value)
        {
            _parameter.VocabularySaveFile = value;
            return this;
        }

        public ParameterBuilder SetCbow(bool cbow)
        {
            _parameter.Cbow = cbow;
            return this;
        }

        public ParameterBuilder SetAlpha(float alpha)
        {
            _alpha = alpha;
            return this;
        }

        public ParameterBuilder SetOutputFile(string outputFile)
        {
            _parameter.OutputFile = outputFile;
            return this;
        }

        public ParameterBuilder SetSkipLengthBetweenWords(int window)
        {
            _parameter.Window = (uint)window;
            return this;
        }

        public ParameterBuilder SetThresholdForOccurrenceOfWords(int value)
        {
            _parameter.Sample = value;
            return this;
        }

        public ParameterBuilder UseHierarchicalSoftmax(bool value)
        {
            _parameter.HierarchicalSoftmax = value;
            return this;
        }

        public ParameterBuilder SetNumberOfNegativeExamples(int value)
        {
            _parameter.NegativeExamples = value;
            return this;
        }

        public ParameterBuilder SetNumberOfThreads(int value)
        {
            _parameter.Threads = value;
            return this;
        }

        public ParameterBuilder SetTrainingIterations(int value)
        {
            _parameter.TrainingIterations = value;
            return this;
        }

        public ParameterBuilder DiscardWordsAppearingLessThan(int value)
        {
            _parameter.MinimalWordCount = value;
            return this;
        }

        public ParameterBuilder OutputWordClasses(int value)
        {
            _parameter.Classes = value;
            return this;
        }

        public Parameter ToParameters()
        {
            var result = _parameter.Clone();
            if (result.Cbow)
            {
                result.Alpha = 0.05f;
            }
            if (_alpha.HasValue)
            {
                result.Alpha = _alpha.Value;
            }
            return result;
        }
    }
}
