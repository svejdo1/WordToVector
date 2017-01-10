# WordToVector
C# implementation of word2vec algorithm. See https://en.wikipedia.org/wiki/Word2vec for details.
Based on C implementation from https://code.google.com/archive/p/word2vec/.

Usage examples:
    
    wget http://mattmahoney.net/dc/text8.zip -O text8.gz
    gzip -d text8.gz -f
(or just download & unzip it)

Train model:
    
    word2vec -train text8 -output vectors.bin -cbow true -size 200 -window 8 -negative 25 -hs false -sample 1e-4 -threads 20 -binary true -iter 15

Test analogies:

    word-analogy vectors.bin
    
Note that for the word analogy to perform well, the model should be trained on much larger data set

Example input: paris france berlin
