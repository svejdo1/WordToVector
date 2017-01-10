using Barbar.WordToVector.Policies;
using System.Collections.Generic;

namespace Barbar.WordToVector.Analogy
{
    public class AnalogyResult<T, TPolicy> where TPolicy : INumberPolicy<T>, new()
    {
        public IList<int> WordNotFoundIndexes { get; set; }
        public IList<WordDistance<T>> Analogies { get; set; }
    }
}
