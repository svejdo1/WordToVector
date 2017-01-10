using Barbar.WordToVector.Policies;
using System;
using System.Collections.Generic;

namespace Barbar.WordToVector.Analogy
{
    public sealed class Vocabulary<T, TPolicy> where TPolicy : INumberPolicy<T>, new()
    {
        private readonly IDictionary<string, Vector<T, TPolicy>> _words;
        private static readonly INumberPolicy<T> s_Policy = new TPolicy();

        public Vocabulary(IDictionary<string, Vector<T, TPolicy>> words)
        {
            if (words == null)
            {
                throw new ArgumentNullException(nameof(words));
            }

            _words = words;
        }

        public AnalogyResult<T, TPolicy> Analogies(string firstWord, string secondWord, string thirdWord, int count)
        {
            if (firstWord == null)
            {
                throw new ArgumentNullException(nameof(firstWord));
            }
            if (secondWord == null)
            {
                throw new ArgumentNullException(nameof(secondWord));
            }
            if (thirdWord == null)
            {
                throw new ArgumentNullException(nameof(thirdWord));
            }
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            var result = new AnalogyResult<T, TPolicy>
            {
                WordNotFoundIndexes = new List<int>()
            };

            var firstVector = Search(firstWord);
            if (firstVector == null)
            {
                result.WordNotFoundIndexes.Add(0);
            }

            var secondVector = Search(secondWord);
            if (secondVector == null)
            {
                result.WordNotFoundIndexes.Add(1);
            }

            var thirdVector = Search(thirdWord);
            if (thirdVector == null)
            {
                result.WordNotFoundIndexes.Add(2);
            }

            if (result.WordNotFoundIndexes.Count > 0)
            {
                return result;
            }

            var targetVector = (secondVector - firstVector + thirdVector).Normalize();
            result.Analogies = new WordDistance<T>[count];

            foreach (var pair in _words)
            {
                if (string.Equals(pair.Key, firstWord, StringComparison.Ordinal) || string.Equals(pair.Key, secondWord, StringComparison.Ordinal) || string.Equals(pair.Key, thirdWord, StringComparison.Ordinal))
                {
                    continue;
                }

                var distance = pair.Value.Distance(targetVector);
                for (int a = 0; a < count; a++)
                {
                    if (result.Analogies[a] == null || s_Policy.Compare(distance, result.Analogies[a].Distance) > 0)
                    {
                        for (var d = count - 1; d > a; d--)
                        {
                            result.Analogies[d] = result.Analogies[d - 1];
                        }
                        result.Analogies[a] = new WordDistance<T> { Word = pair.Key, Distance = distance };
                        break;
                    }
                }
            }

            return result;
        }

        public Vector<T, TPolicy> Search(string word)
        {
            Vector<T, TPolicy> result;
            if (_words.TryGetValue(word, out result))
            {
                return result;
            }
            return null;
        }
    }
}
