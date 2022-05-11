namespace TestRunner
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TestComms;

    public class TestScenarioRunner
    {
        public static async Task<TestResult> Run(string scenarioName, AgentInfo[] agents)
        {
            var uniqueName = scenarioName + Guid.NewGuid().ToString("N");

            using var context = new MemoryMappedFileTestContext(uniqueName, true);

            var processes = agents.Select(x => new AgentProcess(x.Project, x.Behavior, uniqueName, x.BehaviorParameters ?? new Dictionary<string, string>())).ToArray();


            try
            {
                foreach (var agent in processes)
                {
                    agent.Start();
                }

                var finished = await context.WaitUntilTrue("Success", TimeSpan.FromSeconds(1000));
                var variables = context.ToDictionary();
                return new TestResult
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