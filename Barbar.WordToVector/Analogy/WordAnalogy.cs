using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Barbar.WordToVector.Analogy
{
    /*
    public class WordAnalogy
    {
        const int max_size = 2000;         // max length of strings
        const int N = 40;                  // number of closest words that will be shown
        const int max_w = 50;              // max length of vocabulary entries

        private void EnsureRead(JsonTextReader reader, JsonToken token)
        {
            if (!reader.Read())
            {
                throw new WordToVectorException("EOF");
            }
            if (reader.TokenType != token)
            {
                throw new Exception($"{token} expected");
            }
        }

        private IList<VocabularyWord> ReadModelJson(string fileName)
        {
            VocabularyWord[] result;
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
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
                result = new VocabularyWord[wordsCount];
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
                    var word = new VocabularyWord { Value = (string)reader.Value, NormalizedVector = new double[size] };
                    result[b] = word;
                    EnsureRead(reader, JsonToken.StartArray);

                    double len = 0;
                    for (var a = 0; a < size; a++)
                    {
                        double value = reader.ReadAsDouble().Value;
                        word.NormalizedVector[a] = value;
                        len += value * value;
                    }

                    EnsureRead(reader, JsonToken.EndArray);

                    len = Math.Sqrt(len);
                    for (var a = 0; a < size; a++)
                    {
                        word.NormalizedVector[a] /= len;
                    }
                }
            }
            return result;
        }

        private void ReadModel(string fileName, out int size, out string[] vocab, out double[] M)
        {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                string line = reader.ReadLine();
                var regex = new Regex("^(?<words>([0-9]+)) (?<size>([0-9]+))$");
                var match = regex.Match(line);
                var words = int.Parse(match.Groups["words"].Value);
                size = int.Parse(match.Groups["size"].Value);
                vocab = new string[words];
                M = new double[words * size];
                for (var b = 0; b < words; b++)
                {
                    var parts = reader.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != size + 1)
                    {
                        throw new Exception("Invalid line");
                    }
                    vocab[b] = parts[0];
                    if (parts[0][parts[0].Length - 1] == '\0')
                    {
                        vocab[b] = parts[0].Substring(0, parts[0].Length - 1);
                    }

                    for (var a = 0; a < size; a++)
                    {
                        M[a + b * size] = double.Parse(parts[a + 1]);
                        //fread(&M[a + b * size], sizeof(double), 1, f);
                    }
                    double len = 0;
                    for (var a = 0; a < size; a++)
                    {
                        len += M[a + b * size] * M[a + b * size];
                    }
                    len = Math.Sqrt(len);
                    for (var a = 0; a < size; a++)
                    {
                        M[a + b * size] /= len;
                    }
                }
            }
        }

        public WordAnalogyResult Execute(ISet<VocabularyWord> model, string firstWord, string secondWord, string thirdWord)
        {
            var result = new WordAnalogyResult();
            bool exit = false;
            
            if (model == null)
            {
                result.State |= WordAnalogyState.Failure_NoModel;
                exit = true;
            }
            if (string.IsNullOrEmpty(firstWord))
            {
                result.State |= WordAnalogyState.Failure_NoFirstWord;
                exit = true;

            }
            if (string.IsNullOrEmpty(secondWord))
            {
                result.State |= WordAnalogyState.Failure_NoSecondWord;
                exit = true;
            }
            if (string.IsNullOrEmpty(thirdWord))
            {
                result.State |= WordAnalogyState.Failure_NoThirdWord;
                exit = true;
            }

            if (exit)
            {
                return result;
            }



        }

            
        public int Execute(string[] args)
        {
            string st1;
            string[] bestw = new string[N];
            string file_name;
            string[] st = new string[100];
            double dist, len;
            double[] bestd = new double[N];
            double[] vec = new double[max_size];
            int size, a, b, c, d, cn;
            long[] bi = new long[100];
            char ch;
            //double[] normalizedVectors;
            //string[] vocabulary;
            if (args.Length < 1)
            {
                Console.Out.WriteLine("Usage: ./word-analogy <FILE>\nwhere FILE contains word projections in the BINARY FORMAT");
                return 0;
            }
            file_name = args[0];
            //ReadModel(file_name, out size, out vocab, out M);
            var vocabulary = ReadModelJson(file_name);

            while (true)
            {
                for (a = 0; a < N; a++) bestd[a] = 0;
                //for (a = 0; a < N; a++) bestw[a][0] = (char)0;
                Console.Out.WriteLine("Enter three words (EXIT to break): ");
                a = 0;
                st1 = Console.In.ReadLine();
                if (string.Equals(st1, "EXIT", StringComparison.Ordinal))
                    break;
                cn = 0;
                b = 0;
                c = 0;
                st = st1.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                cn = st.Length;
                if (cn < 3)
                {
                    Console.Out.WriteLine($"Only {cn} words were entered.. three words are needed at the input to perform the calculation");
                    continue;
                }
                for (a = 0; a < cn; a++)
                {
                    for (b = 0; b < vocabulary.Count; b++)
                    {
                        if (string.Equals(vocabulary[b].Value, st[a], StringComparison.OrdinalIgnoreCase)) break;
                    }
                    if (b == vocabulary.Length) b = 0;
                    bi[a] = b;
                    Console.Out.WriteLine($"Word: {st[a]} Position in vocabulary: {bi[a]}");
                    if (b == 0)
                    {
                        Console.Out.WriteLine("Out of dictionary word!");
                        break;
                    }
                }
                if (b == 0) continue;
                Console.Out.WriteLine();
                Console.Out.WriteLine("                                              Word              Distance");
                Console.Out.WriteLine("------------------------------------------------------------------------");
                for (a = 0; a < size; a++) vec[a] = normalizedVectors[a + bi[1] * size] - normalizedVectors[a + bi[0] * size] + normalizedVectors[a + bi[2] * size];
                len = 0;
                for (a = 0; a < size; a++) len += vec[a] * vec[a];
                len = Math.Sqrt(len);
                for (a = 0; a < size; a++) vec[a] /= len;
                for (a = 0; a < N; a++) bestd[a] = 0;
                //for (a = 0; a < N; a++) bestw[a][0] = (char)0;
                for (c = 0; c < vocabulary.Length; c++)
                {
                    if (c == bi[0]) continue;
                    if (c == bi[1]) continue;
                    if (c == bi[2]) continue;
                    a = 0;
                    for (b = 0; b < cn; b++) if (bi[b] == c) a = 1;
                    if (a == 1) continue;
                    dist = 0;
                    for (a = 0; a < size; a++) dist += vec[a] * normalizedVectors[a + c * size];
                    for (a = 0; a < N; a++)
                    {
                        if (dist > bestd[a])
                        {
                            for (d = N - 1; d > a; d--)
                            {
                                bestd[d] = bestd[d - 1];
                                bestw[d] = bestw[d - 1];
                            }
                            bestd[a] = dist;
                            bestw[a] = vocabulary[c];
                            break;
                        }
                    }
                }
                for (a = 0; a < N; a++)
                {
                    for (var j = 0; j < Math.Min(bestw[a].Length, 50); j++)
                    {
                        if (bestw[a][j] == 0)
                            break;
                        Console.Out.Write(bestw[a][j]);
                    }
                    if (bestw[a].Length < 50)
                    {
                        Console.Out.Write("".PadLeft(50 - bestw[a].Length, ' '));
                    }

                    Console.Out.WriteLine($"\t\t{bestd[a]}");
                }
            }
            return 0;
        }
    }
    */
}
