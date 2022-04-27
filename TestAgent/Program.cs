namespace TestAgent
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Framework;

    internal class Program
    {
        static Task Main(string[] args)
        {
            return TestAgentFacade.Run(args);
        }
    }
}
