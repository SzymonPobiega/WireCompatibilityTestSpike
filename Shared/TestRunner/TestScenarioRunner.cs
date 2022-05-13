namespace TestRunner
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using TestComms;

    public class TestScenarioRunner
    {
        public static async Task<TestExecutionResult> Run(string scenarioName, AgentInfo[] agents)
        {
            var uniqueName = scenarioName + Guid.NewGuid().ToString("N");
            var tempFile = Path.Combine(Path.GetTempPath(), uniqueName);

            using var context = new MemoryMappedFileTestContext(tempFile, true);

            var processes = agents.Select(x => new AgentProcess(x.Project, x.Behavior, tempFile, x.BehaviorParameters ?? new Dictionary<string, string>())).ToArray();


            try
            {
                foreach (var agent in processes)
                {
                    agent.Start();
                }

                var finished = await context.WaitUntilTrue("Success", TimeSpan.FromSeconds(3000));
                var variables = context.ToDictionary();
                return new TestExecutionResult
                {
                    Succeeded = finished,
                    VariableValues = variables
                };
            }
            finally
            {
                foreach (var agent in processes)
                {
                    await agent.Stop();
                }
            }
            
        }
    }
}