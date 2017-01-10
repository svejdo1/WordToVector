using Barbar.WordToVector.Policies;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Barbar.WordToVector.Analogy
{
    public sealed class VocabularyReader<T, TPolicy> where TPolicy : INumberPolicy<T>, new()
    {
        private static readonly INumberPolicy<T> s_Policy = new TPolicy();

        private void EnsureRead(JsonTextReader reader, JsonToken token)
        {
            if (!reader.Read())
            {
                throw new WordToVectorException("EOF");
            }
            if (reader.TokenType != token)
            {
                throw new WordToVectorException($"{token} expected");
            }
        }

        public IDictionary<string, Vector<T, TPolicy>> ReadToEnd(Stream stream)
        {
            Dictionary<string, Vector<T, TPolicy>> result;

            using (var streamReader = new StreamReader(stream, Encoding.UTF8))
            using (var reader = new JsonTextReader(streamReader))
            {
                EnsureRead(reader, JsonToken.StartObject);
                EnsureRead(reader, JsonToken.PropertyName);
                if (!string.Equals((string)reader.Value, Constants.VocabularySize))
                {
                    throw new WordToVectorException($"'{Constants.VocabularySize}' expected");
                }
                int wordsCount = reader.ReadAsInt32().Value;

                EnsureRead(reader, JsonToken.PropertyName);
                if (!string.Equals((string)reader.Value, Constants.VectorSize))
                {
                    throw new WordToVectorException($"'{Constants.VectorSize}' expected");
                }
                var size = reader.ReadAsInt32().Value;
                result = new Dictionary<string, Vector<T, TPolicy>>(wordsCount);
                //vocab = new string[words];
                //M = new double[words * size];

                EnsureRead(reader, JsonToken.PropertyName);
                if (!string.Equals((string)reader.Value, Constants.Words))
                {
                    throw new WordToVectorException($"'{Constants.Words}' expected");
                }

                EnsureRead(reader, JsonToken.StartObject);

                for (var b = 0; b < wordsCount; b++)
                {
                    EnsureRead(reader, JsonToken.PropertyName);
                    string key = (string)reader.Value;
                    var vector = new Vector<T, TPolicy>(size);
                    EnsureRead(reader, JsonToken.StartArray);

                    T distance = s_Policy.Zero();
                    for (var a = 0; a < size; a++)
                    {
                        var value = s_Policy.ReadFromJson(reader);
                        vector.SetValue(a, value);
                        distance = s_Policy.Add(distance, s_Policy.Multiply(value, value));
                    }

                    EnsureRead(reader, JsonToken.EndArray);

                    distance = s_Policy.Sqrt(distance);
                    for (var a = 0; a < size; a++)
                    {
                        vector.SetValue(a, s_Policy.Divide(vector.GetValue(a), distance));
                    }
                    result.Add(key, vector);
                }
            }
            return result;
        }
    }
}
