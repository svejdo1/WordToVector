using System;
using Barbar.WordToVector.Training;

namespace Barbar.WordToVector.ConsoleApplications
{
    public class TrainingConsoleApplication : IConsoleApplication
    {
        public int Execute(string[] arguments)
        {
            if (arguments == null || arguments.Length == 0)
            {
                Console.Out.WriteLine("WORD VECTOR estimation toolkit v 0.1c");
                Console.Out.WriteLine();
                Console.Out.WriteLine("Options:");
                Console.Out.WriteLine("Parameters for training:");
                Console.Out.WriteLine("\t-train <file>");
                Console.Out.WriteLine("\t\tUse text data from <file> to train the model");
                Console.Out.WriteLine("\t-output <file>");
                Console.Out.WriteLine("\t\tUse <file> to save the resulting word vectors / word clusters");
                Console.Out.WriteLine("\t-size <int>");
                Console.Out.WriteLine("\t\tSet size of word vectors; default is 100");
                Console.Out.WriteLine("\t-window <int>");
                Console.Out.WriteLine("\t\tSet max skip length between words; default is 5");
                Console.Out.WriteLine("\t-sample <float>");
                Console.Out.WriteLine("\t\tSet threshold for occurrence of words. Those that appear with higher frequency in the training data");
                Console.Out.WriteLine("\t\twill be randomly down-sampled; default is 1e-3, useful range is (0, 1e-5)");
                Console.Out.WriteLine("\t-hs <bool>");
                Console.Out.WriteLine("\t\tUse Hierarchical Softmax; default is false (not used)");
                Console.Out.WriteLine("\t-negative <int>");
                Console.Out.WriteLine("\t\tNumber of negative examples; default is 5, common values are 3 - 10 (0 = not used)");
                Console.Out.WriteLine("\t-threads <int>");
                Console.Out.WriteLine("\t\tUse <int> threads (default 12)");
                Console.Out.WriteLine("\t-iter <int>");
                Console.Out.WriteLine("\t\tRun more training iterations (default 5)");
                Console.Out.WriteLine("\t-min-count <int>");
                Console.Out.WriteLine("\t\tThis will discard words that appear less than <int> times; default is 5");
                Console.Out.WriteLine("\t-alpha <float>");
                Console.Out.WriteLine("\t\tSet the starting learning rate; default is 0.025 for skip-gram and 0.05 for CBOW");
                Console.Out.WriteLine("\t-classes <int>");
                Console.Out.WriteLine("\t\tOutput word classes rather than word vectors; default number of classes is 0 (vectors are written)");
                Console.Out.WriteLine("\t-debug <int>");
                Console.Out.WriteLine("\t\tSet the debug mode (default = 2 = more info during training)");
                Console.Out.WriteLine("\t-binary <bool>");
                Console.Out.WriteLine("\t\tSave the resulting vectors in binary moded; default is false (off)");
                Console.Out.WriteLine("\t-save-vocab <file>");
                Console.Out.WriteLine("\t\tThe vocabulary will be saved to <file>");
                Console.Out.WriteLine("\t-read-vocab <file>");
                Console.Out.WriteLine("\t\tThe vocabulary will be read from <file>, not constructed from the training data");
                Console.Out.WriteLine("\t-cbow <bool>");
                Console.Out.WriteLine("\t\tUse the continuous bag of words model; default is true (use false for skip-gram model)");
                Console.Out.WriteLine("\nExamples:");
                Console.Out.WriteLine("./word2vec -train data.txt -output vec.txt -size 200 -window 5 -sample 1e-4 -negative 5 -hs 0 -binary 0 -cbow 1 -iter 3");
                Console.Out.WriteLine();
                return 0;
            }

            //int? i;

            int debug_mode = 2;
            bool binary = false;

            try
            {
                var parameters = new ConsoleParameterBuilder().SetConsoleArguments(arguments).ToParameters();
                new WordTraining().TrainModel(parameters, new ConsoleProgressReport());
            }
            catch (WordToVectorException e)
            {
                Console.Error.WriteLine(e.Message);
                return -1;
            }

            //if ((i = ArgPos("-debug", args)).HasValue) debug_mode = Convert.ToInt32(args[i.Value + 1]);
            //if ((i = ArgPos("-binary", args)).HasValue) binary = Convert.ToBoolean(args[i.Value + 1]);
            
            return 0;
        }
    }
}
