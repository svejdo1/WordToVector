using Barbar.WordToVector.Extensions;
using System;
using System.Runtime.CompilerServices;

namespace Barbar.WordToVector.Training
{
    public class Vocabulary
    {
        private const int vocab_hash_size = 30000000;  // Maximum 30 * 0.7 = 21M words in the vocabulary

        private int[] vocab_hash;
        private VocabularyWord[] vocab;
        private int vocab_size = 0;
        private int vocab_max_size = 1000;
        private int min_reduce = 1;

        public Vocabulary()
        {
            vocab = new VocabularyWord[vocab_max_size];
            vocab_hash = new int[vocab_hash_size];
        }

        public void Reset()
        {
            const int SIZE_OF_INT32 = 4;
            vocab_hash.Memset(-1, SIZE_OF_INT32);
            vocab_size = 0;
        }

        T[] Realloc<T>(T[] oldArray, int newSize)
        {
            int oldSize = oldArray.Length;
            //System.Type elementType = oldArray.GetType().GetElementType();

            T[] newArray = new T[newSize];
            int preserveLength = Math.Min(oldSize, newSize);
            if (preserveLength > 0)
            {
                Array.Copy(oldArray, newArray, preserveLength);
            }
            return newArray;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VocabularyWord GetWord(int index)
        {
            return vocab[index];
        }

        public int Count
        {
            get { return vocab_size; }
        }

        // Sorts the vocabulary by frequency using word counts
        public int SortVocab(int min_count)
        {
            int a, size;
            int hash;
            // Sort the vocabulary and keep Constants.EmptyWord at the first position
            vocab.MergeSort(1, vocab_size - 1, VocabularyWord.CountComparer);

            for (a = 0; a < vocab_hash_size; a++) vocab_hash[a] = -1;
            size = vocab_size;
            int train_words = 0;
            for (a = 0; a < size; a++)
            {
                // Words occuring less than min_count times will be discarded from the vocab
                if ((vocab[a].Count < min_count) && (a != 0))
                {
                    vocab_size--;
                    vocab[a].Word = null;
                }
                else
                {
                    // Hash will be re-computed, as after the sorting it is not actual
                    hash = GetWordHash(vocab[a].Word);
                    while (vocab_hash[hash] != -1) hash = (hash + 1) % vocab_hash_size;
                    vocab_hash[hash] = a;
                    train_words += vocab[a].Count;
                }
            }
            vocab = Realloc(vocab, vocab_size + 1);
            // Allocate memory for the binary tree construction
            for (a = 0; a < vocab_size; a++)
            {
                vocab[a].Code = new bool[Constants.MAX_CODE_LENGTH];
                vocab[a].Point = new int[Constants.MAX_CODE_LENGTH];
            }
            return train_words;
        }

        // Returns hash value of a word
        int GetWordHash(string word)
        {
            uint hash = 0;
            for (var a = 0; a < word.Length; a++)
            {
                hash = hash * 257 + word[a];
            }
            hash = hash % vocab_hash_size;
            return (int)hash;
        }

        // Returns position of a word in the vocabulary; if the word is not found, returns -1
        public int SearchVocab(string word)
        {
            int hash = GetWordHash(word);
            while (true)
            {
                if (vocab_hash[hash] == -1)
                {
                    return -1;
                }
                if (vocab[vocab_hash[hash]].Word == word)
                {
                    return vocab_hash[hash];
                }
                hash = (hash + 1) % vocab_hash_size;
            }
        }

        // Adds a word to the vocabulary
        public int AddWordToVocab(string word)
        {
            var vocabularyWord = new VocabularyWord();
            vocab[vocab_size] = vocabularyWord;
            if (word.Length > Constants.MAX_STRING)
            {
                vocabularyWord.Word = word.Substring(0, Constants.MAX_STRING);
            }
            else
            {
                vocabularyWord.Word = word;
            }
            vocab_size++;
            // Reallocate memory if needed
            if (vocab_size + 2 >= vocab_max_size)
            {
                vocab_max_size += 1000;
                vocab = Realloc(vocab, vocab_max_size);
            }
            var hash = GetWordHash(word);
            while (vocab_hash[hash] != -1)
            {
                hash = (hash + 1) % vocab_hash_size;
            }
            vocab_hash[hash] = vocab_size - 1;
            return vocab_size - 1;
        }

        public void ReduceIfNeeded()
        {
            if (vocab_size > vocab_hash_size * 0.7)
            {
                Reduce();
            }
        }
        
        // Reduces the vocabulary by removing infrequent tokens
        void Reduce()
        {
            int a, b = 0;
            int hash;
            for (a = 0; a < vocab_size; a++)
            {
                if (vocab[a].Count > min_reduce)
                {
                    vocab[b].Count = vocab[a].Count;
                    vocab[b].Word = vocab[a].Word;
                    b++;
                    continue;
                }
                vocab[a].Word = null;
            }
            vocab_size = b;
            for (a = 0; a < vocab_hash_size; a++) vocab_hash[a] = -1;
            for (a = 0; a < vocab_size; a++)
            {
                // Hash will be re-computed, as it is not actual
                hash = GetWordHash(vocab[a].Word);
                while (vocab_hash[hash] != -1) hash = (hash + 1) % vocab_hash_size;
                vocab_hash[hash] = a;
            }
            min_reduce++;
        }

    }
}
