namespace TestRunner
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    class AgentProcess
    {
        readonly Process process;
        Task<string> outputTask;
        Task<string> errorTask;
        bool started;

        public AgentProcess(string projectFilePath, string behaviorType, string mappedFileName, Dictionary<string, string> args)
        {
            process = new Process();
            process.StartInfo.FileName = @"dotnet";
            process.StartInfo.Arguments = $"run --project \"{projectFilePath}\" \"{behaviorType}\" {mappedFileName} {string.Join(" ", args.Select(kvp => FormatArgument(kvp)))}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;
        }

        static string FormatArgument(KeyValuePair<string, string> kvp)
        {
            if (kvp.Value != null)
            {
                return kvp.Key + "=" + kvp.Value;
            }

            return kvp.Key;
        }

        public void Start()
        {
            started = process.Start();

            outputTask = process.StandardOutput.ReadToEndAsync();
            errorTask = process.StandardError.ReadToEndAsync();
        }

        public async Task Stop()
        {
            if (!started)
            {
                return;
            }

            process.Kill();

            var output = await outputTask;
            var error = await errorTask;

            process.Dispose();
        }
    }
}