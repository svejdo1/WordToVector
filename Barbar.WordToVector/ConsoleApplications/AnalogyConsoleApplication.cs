using Barbar.WordToVector.Analogy;
using Barbar.WordToVector.Policies;
using System;
using System.IO;

namespace Barbar.WordToVector.ConsoleApplications
{
    public class AnalogyConsoleApplication : IConsoleApplication
    {
        public int Execute(string[] arguments)
        {
            if (arguments == null || arguments.Length == 0)
            {
                Console.Out.WriteLine("Usage: word-analogy <FILE>\nwhere FILE contains word projections");
                return 0;
            }

            Vocabulary<double, DoublePolicy> vocabulary;
            using (var file = new FileStream(arguments[0], FileMode.Open, FileAccess.Read))
            {
                vocabulary = new Vocabulary<double, DoublePolicy>(new VocabularyReader<double, DoublePolicy>().ReadToEnd(file));
            }

            Console.Out.WriteLine("Enter three words (EXIT to break): ");
            while(true)
            {
                string line = Console.In.ReadLine();
                if (string.Compare(line, "EXIT", StringComparison.Ordinal) == 0)
                {
                    return 0;
                }
                var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3)
                {
                    Console.Out.WriteLine($"Only {parts.Length} words were entered.. three words are needed at the input to perform the calculation");
                    continue;
                }

                var analogies = vocabulary.Analogies(parts[0], parts[1], parts[2], 50);
                if (analogies.WordNotFoundIndexes != null && analogies.WordNotFoundIndexes.Count > 0) 
                {
                    foreach(var index in analogies.WordNotFoundIndexes)
                    {
                        Console.Out.WriteLine($"Word '{parts[index]}' was not found in dictionary.");
                    }
                    continue;
                }

                foreach (var item in analogies.Analogies)
                {
                    Console.Out.Write(item.Word.PadRight(30, ' '));
                    Console.Out.Write("\t\t");
                    Console.Out.Write(item.Distance);
                    Console.Out.WriteLine();
                }
            }
        }
    }
}
