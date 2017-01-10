using Barbar.WordToVector.Policies;
using System;

namespace Barbar.WordToVector.Analogy
{
    public class VocabularyWord<T, TPolicy> where TPolicy : INumberPolicy<T>, new()
    {
        public string Value { get; set; }
        public Vector<T, TPolicy> NormalizedVector { get; set; }

        public override bool Equals(object obj)
        {
            var word = (VocabularyWord<T, TPolicy>)obj;
            return string.Equals(Value, word.Value, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator == (VocabularyWord<T, TPolicy> a, VocabularyWord<T, TPolicy> b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(VocabularyWord<T, TPolicy> a, VocabularyWord<T, TPolicy> b)
        {
            return !a.Equals(b);
        }
    }
}
