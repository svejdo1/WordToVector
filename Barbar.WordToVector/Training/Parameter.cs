namespace Barbar.WordToVector.Training
{
    public class Parameter
    {
        public int VectorSize { get; set; } = 100;
        public string TrainFile { get; set; }
        public string VocabularySaveFile { get; set; }
        public string VocabularyReadFile { get; set; }
        public bool Cbow { get; set; } = true;
        public float Alpha { get; set; } = 0.025f;
        public string OutputFile { get; set; }
        public uint Window { get; set; } = 5;
        public float Sample { get; set; } = 1e-3f;
        public int NegativeExamples { get; set; } = 5;
        public bool HierarchicalSoftmax { get; set; }
        public int Threads { get; set; } = 12;
        public int TrainingIterations { get; set; } = 5;
        public int MinimalWordCount { get; set; } = 5;
        public int Classes { get; set; }

        public Parameter Clone()
        {
            return (Parameter)MemberwiseClone();
        }
    }
}
