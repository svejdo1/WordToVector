using Barbar.WordToVector.ConsoleApplications;

namespace Barbar.WordToVectorTraining
{
    class Program
    {
        static int Main(string[] args)
        {
            var application = new TrainingConsoleApplication();
            return application.Execute(args);
        }
    }
}
