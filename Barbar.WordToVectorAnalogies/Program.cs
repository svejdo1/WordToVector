using Barbar.WordToVector.ConsoleApplications;

namespace Barbar.WordToVectorAnalogies
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var application = new AnalogyConsoleApplication();
            return application.Execute(args);
        }
    }
}
