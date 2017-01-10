//  Copyright 2013 Google Inc. All Rights Reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Barbar.WordToVector.Training
{
    public class WordTraining
    {

        public const int EXP_TABLE_SIZE = 1000;
        public const int MAX_EXP = 6;
        public const int MAX_SENTENCE_LENGTH = 1000;
        

        int train_words = 0, word_count_actual = 0, file_size = 0;
        float alpha;
        float starting_alpha;
        float[] syn0, syn1, syn1neg;
        DateTime start;

        const int table_size = (int)1e8;
        int[] table;

        void InitUnigramTable(Vocabulary vocabulary)
        {
            int i;
            double train_words_pow = 0;
            double d1, power = 0.75;
            table = new int[table_size];
            for (var a = 0; a < vocabulary.Count; a++)
            {
                train_words_pow += Math.Pow(vocabulary.GetWord(a).Count, power);
            }
            i = 0;
            d1 = Math.Pow(vocabulary.GetWord(i).Count, power) / train_words_pow;
            for (var a = 0; a < table_size; a++)
            {
                table[a] = i;
                if (a / (double)table_size > d1)
                {
                    i++;
                    d1 += Math.Pow(vocabulary.GetWord(i).Count, power) / train_words_pow;
                }
                if (i >= vocabulary.Count)
                {
                    i = vocabulary.Count - 1;
                }
            }
        }

        //// Reads a single word from a file, assuming space + tab + EOL to be word boundaries
        string ReadWord(StreamReader fin)
        {
            char[] result = new char[Constants.MAX_STRING];
            int length = 0;
            while (!fin.EndOfStream)
            {
                var ch = fin.Peek();
                if (ch == 13)
                {
                    fin.Read();
                    continue;
                }
                if ((ch == ' ') || (ch == '\t') || (ch == '\n'))
                {
                    if (length > 0)
                    {
                        if (ch == '\n')
                        {
                            break;
                        }
                        fin.Read();
                        break;
                    }
                    fin.Read();
                    if (ch == '\n')
                    {
                        return Constants.EmptyWord;
                    }
                    continue;
                }
                fin.Read();
                if (length < Constants.MAX_STRING - 1)
                {
                    result[length] = (char)ch;
                }
                length++;
            }
            return new string(result, 0, Math.Min(length, Constants.MAX_STRING));
        }

        // Reads a word and returns its index in the vocabulary
        int ReadWordIndex(StreamReader fin, Vocabulary vocabulary)
        {
            string word = ReadWord(fin);
            if (fin.EndOfStream)
            {
                return -1;
            }
            return vocabulary.SearchVocab(word);
        }

        // Create binary Huffman tree using the word counts
        // Frequent words will have short uniqe binary codes
        void CreateBinaryTree(Vocabulary vocabulary)
        {
            int[] point = new int[Constants.MAX_CODE_LENGTH];
            bool[] code = new bool[Constants.MAX_CODE_LENGTH];
            long[] count = new long[vocabulary.Count * 2 + 1];
            bool[] binary = new bool[vocabulary.Count * 2 + 1];
            int[] parentNode = new int[vocabulary.Count * 2 + 1];
            for (var a = 0; a < vocabulary.Count; a++)
            {
                count[a] = vocabulary.GetWord(a).Count;
            }
            for (var a = vocabulary.Count; a < vocabulary.Count * 2; a++)
            {
                count[a] = (long)1e15;
            }
            var pos1 = vocabulary.Count - 1;
            var pos2 = vocabulary.Count;
            int min1i, min2i;
            // Following algorithm constructs the Huffman tree by adding one node at a time
            for (var a = 0; a < vocabulary.Count - 1; a++)
            {
                // First, find two smallest nodes 'min1, min2'
                if (pos1 >= 0)
                {
                    if (count[pos1] < count[pos2])
                    {
                        min1i = pos1;
                        pos1--;
                    }
                    else
                    {
                        min1i = pos2;
                        pos2++;
                    }
                }
                else
                {
                    min1i = pos2;
                    pos2++;
                }
                if (pos1 >= 0)
                {
                    if (count[pos1] < count[pos2])
                    {
                        min2i = pos1;
                        pos1--;
                    }
                    else
                    {
                        min2i = pos2;
                        pos2++;
                    }
                }
                else
                {
                    min2i = pos2;
                    pos2++;
                }
                count[vocabulary.Count + a] = count[min1i] + count[min2i];
                parentNode[min1i] = vocabulary.Count + a;
                parentNode[min2i] = vocabulary.Count + a;
                binary[min2i] = true;
            }
            // Now assign binary code to each vocabulary word
            for (var a = 0; a < vocabulary.Count; a++)
            {
                var b = a;
                var i = 0;
                while (true)
                {
                    code[i] = binary[b];
                    point[i] = b;
                    i++;
                    b = parentNode[b];
                    if (b == vocabulary.Count * 2 - 2)
                    {
                        break;
                    }
                }
                vocabulary.GetWord(a).CodeLength = (byte)i;
                vocabulary.GetWord(a).Point[0] = vocabulary.Count - 2;
                for (var c = 0; c < i; c++)
                {
                    vocabulary.GetWord(a).Code[i - c - 1] = code[c];
                    vocabulary.GetWord(a).Point[i - c] = point[c] - vocabulary.Count;
                }
            }
        }

        void LearnVocabFromTrainFile(Parameter parameters, Vocabulary vocabulary, IProgressReport progressReport)
        {
            int a, i;
            vocabulary.Reset();
            try
            {
                using (var stream = new FileStream(parameters.TrainFile, FileMode.Open, FileAccess.Read))
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {

                    vocabulary.AddWordToVocab(Constants.EmptyWord);
                    while (true)
                    {
                        string word = ReadWord(reader);
                        if (reader.EndOfStream)
                        {
                            break;
                        }
                        train_words++;
                        if (progressReport != null && train_words % 100000 == 0)
                        {
                            progressReport.OnWordsTrainedProgress(train_words);
                        }
                        i = vocabulary.SearchVocab(word);
                        if (i == -1)
                        {
                            a = vocabulary.AddWordToVocab(word);
                            vocabulary.GetWord(a).Count = 1;
                        }
                        else
                        {
                            vocabulary.GetWord(i).Count++;
                        }
                        vocabulary.ReduceIfNeeded();
                    }
                    train_words = vocabulary.SortVocab(parameters.MinimalWordCount);
                    if (progressReport != null)
                    {
                        progressReport.OnWordsTrainedFinished(train_words, vocabulary.Count);
                    }
                    file_size = (int)stream.Position;
                }
            }
            catch(FileNotFoundException e)
            {
                throw new WordToVectorException($"File \"{parameters.TrainFile}\" was not found.", e);
            }
        }

        void SaveVocab(string fileName, Vocabulary vocabulary)
        {
            using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                for (var i = 0; i < vocabulary.Count; i++)
                {
                    writer.Write(vocabulary.GetWord(i).Word);
                    writer.Write($" {vocabulary.GetWord(i).Count}\n");
                }
            }
        }

        void ReadVocab(Vocabulary vocabulary, string train_file, string read_vocab_file, int min_count, IProgressReport progressReport)
        {
            //int i = 0;
            vocabulary.Reset();

            using (var stream = new FileStream(read_vocab_file, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {

                while (true)
                {
                    string word = ReadWord(reader);
                    if (reader.EndOfStream)
                    {
                        break;
                    }
                    var a = vocabulary.AddWordToVocab(word);
                    // fscanf(fin, "%lld%c", &vocab[a].cn, &c);
                    throw new NotImplementedException("TODO");
                    //vocab[a].cn = 0;
                    //do
                    //{
                    //  char c = fin.Read();

                    //}

                    //i++;
                }
                vocabulary.SortVocab(min_count);
                if (progressReport != null)
                {
                    progressReport.OnWordsTrainedFinished(train_words, vocabulary.Count);
                }
            }

            file_size = (int)new FileInfo(train_file).Length;
        }

        void InitNetwork(Vocabulary vocabulary, int layer1_size, bool hs, int negative)
        {
            ulong next_random = 1L;
            int syn0size = vocabulary.Count * layer1_size;
            syn0 = new float[syn0size];
            if (hs)
            {
                syn1 = new float[vocabulary.Count * layer1_size];
            }
            if (negative > 0)
            {
                syn1neg = new float[vocabulary.Count * layer1_size];
            }
            for (var a = 0; a < vocabulary.Count; a++)
            {
                for (var b = 0; b < layer1_size; b++)
                {
                    next_random = next_random * (ulong)25214903917 + 11;
                    syn0[a * layer1_size + b] = (((next_random & 0xFFFF) / (float)65536) - 0.5f) / layer1_size;
                }
            }
            CreateBinaryTree(vocabulary);
        }

        void TrainModelThread(int threadIndex, Vocabulary vocabulary, Parameter parameters, float[] expTable, IProgressReport progressReport)
        {
            int a, b, d, cw, word, last_word, sentence_length = 0, sentence_position = 0;
            int word_count = 0, last_word_count = 0;
            int[] sen = new int[MAX_SENTENCE_LENGTH + 1];
            int l1, l2, c, target, label, local_iter = parameters.TrainingIterations;
            ulong next_random = (ulong)threadIndex;
            float f, g;
            DateTime now;
            float[] neu1 = new float[parameters.VectorSize];
            float[] neu1e = new float[parameters.VectorSize];

            using (var stream = new FileStream(parameters.TrainFile, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                stream.Seek(file_size / parameters.Threads * threadIndex, SeekOrigin.Begin);


                while (true)
                {
                    if (word_count - last_word_count > 10000)
                    {
                        word_count_actual += word_count - last_word_count;
                        last_word_count = word_count;
                        if (progressReport != null)
                        {
                            now = DateTime.Now;
                            var progress = word_count_actual / (double)(parameters.TrainingIterations * train_words + 1) * 100;
                            var persec = word_count_actual / ((double)(now - start).TotalSeconds + 1);
                            progressReport.OnVectorsTrainedProgress(alpha, progress, persec);
                        }
                        alpha = starting_alpha * (1 - word_count_actual / (float)(parameters.TrainingIterations * train_words + 1));
                        if (alpha < starting_alpha * 0.0001)
                        {
                            alpha = starting_alpha * 0.0001f;
                        }
                    }
                    if (sentence_length == 0)
                    {
                        while (true)
                        {
                            word = ReadWordIndex(reader, vocabulary);
                            if (reader.EndOfStream)
                            {
                                break;
                            }
                            if (word == -1)
                            {
                                continue;
                            }
                            word_count++;
                            if (word == 0)
                            {
                                break;
                            }
                            // The subsampling randomly discards frequent words while keeping the ranking same
                            if (parameters.Sample > 0)
                            {
                                float ran = (float)(Math.Sqrt(vocabulary.GetWord(word).Count / (parameters.Sample * train_words)) + 1) * (parameters.Sample * train_words) / vocabulary.GetWord(word).Count;
                                next_random = next_random * (ulong)25214903917 + 11;
                                if (ran < (next_random & 0xFFFF) / (float)65536)
                                {
                                    continue;
                                }
                            }
                            sen[sentence_length] = word;
                            sentence_length++;
                            if (sentence_length >= MAX_SENTENCE_LENGTH) break;
                        }
                        sentence_position = 0;
                    }
                    if (reader.EndOfStream || (word_count > train_words / parameters.Threads))
                    {
                        word_count_actual += word_count - last_word_count;
                        local_iter--;
                        if (local_iter == 0) break;
                        word_count = 0;
                        last_word_count = 0;
                        sentence_length = 0;
                        stream.Seek(file_size / (int)parameters.Threads * (int)threadIndex, SeekOrigin.Begin);
                        continue;
                    }
                    word = sen[sentence_position];
                    if (word == -1) continue;
                    for (c = 0; c < parameters.VectorSize; c++) neu1[c] = 0;
                    for (c = 0; c < parameters.VectorSize; c++) neu1e[c] = 0;
                    next_random = next_random * (ulong)25214903917 + 11;
                    b = (int)(next_random % parameters.Window);
                    if (parameters.Cbow)
                    {  //train the cbow architecture
                       // in -> hidden
                        cw = 0;
                        for (a = b; a < parameters.Window * 2 + 1 - b; a++)
                        {
                            if (a != parameters.Window)
                            {
                                c = (int)(sentence_position - parameters.Window + a);
                                if (c < 0) continue;
                                if (c >= sentence_length) continue;
                                last_word = sen[c];
                                if (last_word == -1) continue;
                                for (c = 0; c < parameters.VectorSize; c++)
                                {
                                    neu1[c] += syn0[c + last_word * parameters.VectorSize];
                                }
                                cw++;
                            }
                        }
                        if (cw != 0)
                        {
                            for (c = 0; c < parameters.VectorSize; c++)
                            {
                                neu1[c] /= cw;
                            }
                            if (parameters.HierarchicalSoftmax)
                            {
                                for (d = 0; d < vocabulary.GetWord(word).CodeLength; d++)
                                {
                                    f = 0;
                                    l2 = vocabulary.GetWord(word).Point[d] * parameters.VectorSize;
                                    // Propagate hidden -> output
                                    for (c = 0; c < parameters.VectorSize; c++)
                                    {
                                        f += neu1[c] * syn1[c + l2];
                                    }
                                    if (f <= -MAX_EXP) continue;
                                    else if (f >= MAX_EXP) continue;
                                    else f = expTable[(int)((f + MAX_EXP) * (EXP_TABLE_SIZE / MAX_EXP / 2))];
                                    // 'g' is the gradient multiplied by the learning rate
                                    g = ((vocabulary.GetWord(word).Code[d] ? 0 : 1) - f) * alpha;
                                    // Propagate errors output -> hidden
                                    for (c = 0; c < parameters.VectorSize; c++)
                                    {
                                        neu1e[c] += g * syn1[c + l2];
                                    }
                                    // Learn weights hidden -> output
                                    for (c = 0; c < parameters.VectorSize; c++)
                                    {
                                        syn1[c + l2] += g * neu1[c];
                                    }
                                }
                            }
                            // NEGATIVE SAMPLING
                            if (parameters.NegativeExamples > 0)
                            {
                                for (d = 0; d < parameters.NegativeExamples + 1; d++)
                                {
                                    if (d == 0)
                                    {
                                        target = word;
                                        label = 1;
                                    }
                                    else
                                    {
                                        next_random = next_random * (ulong)25214903917 + 11;
                                        target = table[(next_random >> 16) % table_size];
                                        if (target == 0)
                                        {
                                            target = (int)(next_random % (uint)(vocabulary.Count - 1) + 1);
                                        }
                                        if (target == word) continue;
                                        label = 0;
                                    }
                                    l2 = target * parameters.VectorSize;
                                    f = 0;
                                    for (c = 0; c < parameters.VectorSize; c++)
                                    {
                                        f += neu1[c] * syn1neg[c + l2];
                                    }
                                    if (f > MAX_EXP) g = (label - 1) * alpha;
                                    else if (f < -MAX_EXP) g = (label - 0) * alpha;
                                    else g = (label - expTable[(int)((f + MAX_EXP) * (EXP_TABLE_SIZE / MAX_EXP / 2))]) * alpha;
                                    for (c = 0; c < parameters.VectorSize; c++)
                                    {
                                        neu1e[c] += g * syn1neg[c + l2];
                                    }
                                    for (c = 0; c < parameters.VectorSize; c++)
                                    {
                                        syn1neg[c + l2] += g * neu1[c];
                                    }
                                }
                            }
                            // hidden -> in
                            for (a = b; a < parameters.Window * 2 + 1 - b; a++)
                            {
                                if (a != parameters.Window)
                                {
                                    c = (int)(sentence_position - parameters.Window + a);
                                    if (c < 0) continue;
                                    if (c >= sentence_length) continue;
                                    last_word = sen[c];
                                    if (last_word == -1) continue;
                                    for (c = 0; c < parameters.VectorSize; c++)
                                    {
                                        syn0[c + last_word * parameters.VectorSize] += neu1e[c];
                                    }
                                }
                            }
                        }
                    }
                    else
                    {  //train skip-gram 
                        for (a = b; a < parameters.Window * 2 + 1 - b; a++)
                        {
                            if (a != parameters.Window)
                            {
                                c = (int)(sentence_position - parameters.Window + a);
                                if (c < 0 || c >= sentence_length)
                                {
                                    continue;
                                }
                                last_word = sen[c];
                                if (last_word == -1) continue;
                                l1 = last_word * parameters.VectorSize;
                                for (c = 0; c < parameters.VectorSize; c++)
                                {
                                    neu1e[c] = 0;
                                }
                                // HIERARCHICAL SOFTMAX
                                if (parameters.HierarchicalSoftmax)
                                {
                                    for (d = 0; d < vocabulary.GetWord(word).CodeLength; d++)
                                    {
                                        f = 0;
                                        l2 = vocabulary.GetWord(word).Point[d] * parameters.VectorSize;
                                        // Propagate hidden -> output
                                        for (c = 0; c < parameters.VectorSize; c++)
                                        {
                                            f += syn0[c + l1] * syn1[c + l2];
                                        }
                                        if (f <= -MAX_EXP) continue;
                                        else if (f >= MAX_EXP) continue;
                                        else f = expTable[(int)((f + MAX_EXP) * (EXP_TABLE_SIZE / MAX_EXP / 2))];
                                        // 'g' is the gradient multiplied by the learning rate
                                        g = ((vocabulary.GetWord(word).Code[d] ? 0 : 1) - f) * alpha;
                                        // Propagate errors output -> hidden
                                        for (c = 0; c < parameters.VectorSize; c++) neu1e[c] += g * syn1[c + l2];
                                        // Learn weights hidden -> output
                                        for (c = 0; c < parameters.VectorSize; c++) syn1[c + l2] += g * syn0[c + l1];
                                    }
                                }
                                // NEGATIVE SAMPLING
                                if (parameters.NegativeExamples > 0)
                                {
                                    for (d = 0; d < parameters.NegativeExamples + 1; d++)
                                    {
                                        if (d == 0)
                                        {
                                            target = word;
                                            label = 1;
                                        }
                                        else
                                        {
                                            next_random = next_random * (ulong)25214903917 + 11;
                                            target = table[(next_random >> 16) % table_size];
                                            if (target == 0) target = (int)(next_random % (uint)(vocabulary.Count - 1) + 1);
                                            if (target == word) continue;
                                            label = 0;
                                        }
                                        l2 = target * parameters.VectorSize;
                                        f = 0;
                                        for (c = 0; c < parameters.VectorSize; c++)
                                        {
                                            f += syn0[c + l1] * syn1neg[c + l2];
                                        }
                                        if (f > MAX_EXP) g = (label - 1) * alpha;
                                        else if (f < -MAX_EXP) g = (label - 0) * alpha;
                                        else g = (label - expTable[(int)((f + MAX_EXP) * (EXP_TABLE_SIZE / MAX_EXP / 2))]) * alpha;
                                        for (c = 0; c < parameters.VectorSize; c++) neu1e[c] += g * syn1neg[c + l2];
                                        for (c = 0; c < parameters.VectorSize; c++) syn1neg[c + l2] += g * syn0[c + l1];
                                    }
                                }
                                // Learn weights input -> hidden
                                for (c = 0; c < parameters.VectorSize; c++)
                                {
                                    syn0[c + l1] += neu1e[c];
                                }
                            }
                    }
                    }
                    sentence_position++;
                    if (sentence_position >= sentence_length)
                    {
                        sentence_length = 0;
                        continue;
                    }
                }
            }
        }

        void SaveWordVectorsAsJson(string output_file, Vocabulary vocabulary, int vectorSize)
        {
            using (var stream = new FileStream(output_file, FileMode.Create, FileAccess.Write))
            using (var streamWriter = new StreamWriter(stream, new UTF8Encoding(false)))
            using (var writer = new JsonTextWriter(streamWriter))
            {
                // Save the word vectors
                // fprintf(fo, "%lld %lld\n", vocab_size, layer1_size);
                writer.WriteStartObject();
                writer.WritePropertyName(Constants.VocabularySize);
                writer.WriteValue(vocabulary.Count);
                writer.WritePropertyName(Constants.VectorSize);
                writer.WriteValue(vectorSize);
                writer.WritePropertyName(Constants.Words);
                writer.WriteStartObject();
                for (var a = 0; a < vocabulary.Count; a++)
                {

                    writer.WritePropertyName(vocabulary.GetWord(a).Word);
                    writer.WriteStartArray();
                    //writer.Write(" ");
                    //if (binary)
                    //{
                    //    for (var b = 0; b < layer1_size; b++)
                    //    {
                    //        //fwrite(&syn0[a * layer1_size + b], sizeof(double), 1, fo);
                    //        throw new NotImplementedException("TODO");
                    //    }
                    //    writer.WriteLine();
                    //    continue;
                    //}

                    for (var b = 0; b < vectorSize; b++)
                    {
                        writer.WriteValue(syn0[a * vectorSize + b]);
                        //writer.Write(syn0[a * layer1_size + b]);
                        //writer.Write(" ");
                        //fprintf(fo, "%lf ", syn0[a * layer1_size + b]);
                    }
                    writer.WriteEndArray();
                }
                writer.WriteEndObject();
                writer.WriteEndObject();
            }
        }

        void SaveWordVectors(string output_file, Vocabulary vocabulary, int firstLayerSize, bool binary)
        {
            using (var stream = new FileStream(output_file, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                // Save the word vectors
                // fprintf(fo, "%lld %lld\n", vocab_size, layer1_size);
                writer.WriteLine($"{vocabulary.Count} {firstLayerSize}");
                for (var a = 0; a < vocabulary.Count; a++)
                {
                    writer.Write(vocabulary.GetWord(a).Word);
                    writer.Write(" ");
                    if (binary)
                    {
                        for (var b = 0; b < firstLayerSize; b++)
                        {
                            //fwrite(&syn0[a * layer1_size + b], sizeof(double), 1, fo);
                            throw new NotImplementedException("TODO");
                        }
                        writer.WriteLine();
                        continue;
                    }

                    for (var b = 0; b < firstLayerSize; b++)
                    {
                        writer.Write(syn0[a * firstLayerSize + b]);
                        writer.Write(" ");
                        //fprintf(fo, "%lf ", syn0[a * layer1_size + b]);
                    }
                    writer.WriteLine();
                }
            }
        }

        public void TrainModel(Parameter parameters, IProgressReport progressReport)
        {
            alpha = parameters.Alpha;

            var vocabulary = new Vocabulary();

            Console.Out.WriteLine($"Starting training using file {parameters.TrainFile}\n");
            starting_alpha = alpha;
            if (!string.IsNullOrEmpty(parameters.VocabularyReadFile))
            {
                ReadVocab(vocabulary, parameters.TrainFile, parameters.VocabularyReadFile, parameters.MinimalWordCount, progressReport);
            }
            else
            {
                LearnVocabFromTrainFile(parameters, vocabulary, progressReport);
            }
            if (!string.IsNullOrEmpty(parameters.VocabularySaveFile))
            {
                SaveVocab(parameters.VocabularySaveFile, vocabulary);
            }
            if (string.IsNullOrEmpty(parameters.OutputFile))
            {
                return;
            }
            InitNetwork(vocabulary, parameters.VectorSize, parameters.HierarchicalSoftmax, parameters.NegativeExamples);
            if (parameters.NegativeExamples > 0)
            {
                InitUnigramTable(vocabulary);
            }
            var expTable = GenerateExpTable();

            start = DateTime.Now;
            var pt = new Task[parameters.Threads];
            for (var a = 0; a < parameters.Threads; a++)
            {
                int threadIndex = a;
                pt[a] = new TaskFactory().StartNew(() => TrainModelThread(threadIndex, vocabulary, parameters, expTable, progressReport));
            }
            Task.WaitAll(pt);

            if (parameters.Classes == 0)
            {
                //SaveWordVectors();
                SaveWordVectorsAsJson(parameters.OutputFile, vocabulary, parameters.VectorSize);
                return;
            }


            // Run K-means on the word vectors
            int clcn = parameters.Classes, iter = 10, closeid;
            int[] centcn = new int[parameters.Classes];
            int[] cl = new int[vocabulary.Count];
            double closev, x;
            double[] cent = new double[parameters.Classes * parameters.VectorSize];
            for (var a = 0; a < vocabulary.Count; a++)
            {
                cl[a] = (int)(a % clcn);
            }
            for (var a = 0; a < iter; a++)
            {
                for (var b = 0; b < clcn * parameters.VectorSize; b++)
                {
                    cent[b] = 0;
                }
                for (var b = 0; b < clcn; b++)
                {
                    centcn[b] = 1;
                }
                for (var c = 0; c < vocabulary.Count; c++)
                {
                    for (var d = 0; d < parameters.VectorSize; d++)
                    {
                        cent[parameters.VectorSize * cl[c] + d] += syn0[c * parameters.VectorSize + d];
                    }
                    centcn[cl[c]]++;
                }
                for (var b = 0; b < clcn; b++)
                {
                    closev = 0;
                    for (var c = 0; c < parameters.VectorSize; c++)
                    {
                        cent[parameters.VectorSize * b + c] /= centcn[b];
                        closev += cent[parameters.VectorSize * b + c] * cent[parameters.VectorSize * b + c];
                    }
                    closev = Math.Sqrt(closev);
                    for (var c = 0; c < parameters.VectorSize; c++)
                    {
                        cent[parameters.VectorSize * b + c] /= closev;
                    }
                }
                for (var c = 0; c < vocabulary.Count; c++)
                {
                    closev = -10;
                    closeid = 0;
                    for (var d = 0; d < clcn; d++)
                    {
                        x = 0;
                        for (var b = 0; b < parameters.VectorSize; b++)
                        {
                            x += cent[parameters.VectorSize * d + b] * syn0[c * parameters.VectorSize + b];
                        }
                        if (x > closev)
                        {
                            closev = x;
                            closeid = (int)d;
                        }
                    }
                    cl[c] = closeid;
                }
            }
            using (var stream = new FileStream(parameters.OutputFile, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                // Save the K-means classes
                for (var a = 0; a < vocabulary.Count; a++)
                {
                    writer.WriteLine($"{vocabulary.GetWord(a).Word} {cl[a]}");
                }
            }
        }

        public static float[] GenerateExpTable()
        {
            var expTable = new float[EXP_TABLE_SIZE + 1];
            for (var j = 0; j < EXP_TABLE_SIZE; j++)
            {
                expTable[j] = (float)Math.Exp((j / (float)EXP_TABLE_SIZE * 2 - 1) * MAX_EXP); // Precompute the exp() table
                expTable[j] = expTable[j] / (expTable[j] + 1);                   // Precompute f(x) = x / (x + 1)
            }
            return expTable;
        }
    }
}