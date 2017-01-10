using System;

namespace Barbar.WordToVector.Training
{
    public class ConsoleParameterBuilder : IParameterBuilder
    {
        private Parameter _parameter = new Parameter();
        private float? _alpha;

        static int? ArgPos(string argument, string[] arguments)
        {
            for (var a = 0; a < arguments.Length; a++)
            {
                if (string.Equals(argument, arguments[a], StringComparison.OrdinalIgnoreCase))
                {
                    if (a == arguments.Length - 1)
                    {
                        throw new WordToVectorException($"Argument missing for {argument}");
                    }
                    return a;
                }
            }
            return null;
        }

        public ConsoleParameterBuilder SetConsoleArguments(string[] args)
        {
            int? i;
            if ((i = ArgPos("-size", args)).HasValue) _parameter.VectorSize = Convert.ToInt32(args[i.Value + 1]);
            if ((i = ArgPos("-train", args)).HasValue) _parameter.TrainFile = args[i.Value + 1];
            if ((i = ArgPos("-save-vocab", args)).HasValue) _parameter.VocabularySaveFile = args[i.Value + 1];
            if ((i = ArgPos("-read-vocab", args)).HasValue) _parameter.VocabularyReadFile = args[i.Value + 1];
            //if ((i = ArgPos("-debug", args)).HasValue) debug_mode = Convert.ToInt32(args[i.Value + 1]);
            //if ((i = ArgPos("-binary", args)).HasValue) binary = Convert.ToBoolean(args[i.Value + 1]);
            if ((i = ArgPos("-cbow", args)).HasValue) _parameter.Cbow = Convert.ToBoolean(args[i.Value + 1]);

            if ((i = ArgPos("-alpha", args)).HasValue) _alpha = Convert.ToSingle(args[i.Value + 1]);
            if ((i = ArgPos("-output", args)).HasValue) _parameter.OutputFile = args[i.Value + 1];
            if ((i = ArgPos("-window", args)).HasValue) _parameter.Window = (uint)Convert.ToInt32(args[i.Value + 1]);
            if ((i = ArgPos("-sample", args)).HasValue) _parameter.Sample = Convert.ToSingle(args[i.Value + 1]);
            if ((i = ArgPos("-hs", args)).HasValue) _parameter.HierarchicalSoftmax = Convert.ToBoolean(args[i.Value + 1]);
            if ((i = ArgPos("-negative", args)).HasValue) _parameter.NegativeExamples = Convert.ToInt32(args[i.Value + 1]);
            if ((i = ArgPos("-threads", args)).HasValue) _parameter.Threads = Convert.ToInt32(args[i.Value + 1]);
            if ((i = ArgPos("-iter", args)).HasValue) _parameter.TrainingIterations = Convert.ToInt32(args[i.Value + 1]);
            if ((i = ArgPos("-min-count", args)).HasValue) _parameter.MinimalWordCount = Convert.ToInt32(args[i.Value + 1]);
            if ((i = ArgPos("-classes", args)).HasValue) _parameter.Classes = Convert.ToInt32(args[i.Value + 1]);

            return this;
        }

        public Parameter ToParameters()
        {
            var result = _parameter.Clone();
            if (result.Cbow)
            {
                result.Alpha = 0.05f;
            }
            if (_alpha.HasValue)
            {
                result.Alpha = _alpha.Value;
            }
            return result;
        }
    }
}
