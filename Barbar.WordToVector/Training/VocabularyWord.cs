using System.Collections.Generic;

namespace Barbar.WordToVector.Training
{
    public sealed class VocabularyWord
    {
        public int Count { get; set; }
        public string Word { get; set; }
        public int[] Point { get; set; }
        public bool[] Code { get; set; }
        public byte CodeLength { get; set; }

        public static readonly IComparer<VocabularyWord> CountComparer = new CountComparerImplementation();

        private sealed class CountComparerImplementation : IComparer<VocabularyWord>
        {
            public int Compare(VocabularyWord x, VocabularyWord y)
            {
                return Comparer<int>.Default.Compare(y.Count, x.Count);
            }
        }
    }
}
