namespace TestSuite
{
    using System.Threading.Tasks;
    using NUnit.Framework;
    using TestRunner;

    [TestFixture]
    public class ThisIsATest
    {
        [Test]
        public async Task RunHardcodedScenario()
        {
            var result = await TestScenarioRunner.Run("Ping-Pong", new[]
            {
                new AgentInfo()
                {
                    Behavior = "TestBehaviors.V7.Sender, TestBehaviors.V7",
                    Project = "TestAgent.V7"
                },
                new AgentInfo()
                {
                    Behavior = "TestBehaviors.V8.Receiver, TestBehaviors.V8",
                    Project = "TestAgent.V8"
                },
            });
            Assert.True(result.Succeeded);

            /*
             * The spike implementation of the shared ITestContextAccessor is
             * based on a Dictionary<string, int>. It allows only for 0 or 1 named
             * flags. When setting the flag using the boolean overload, the value
             * will be converted to int. his is why it must be consumed as int. 
             */
            var requestReceived = (int)result.VariableValues["RequestReceived"];
            var responseReceived = (int)result.VariableValues["ResponseReceived"];

            Assert.AreEqual(1, requestReceived);
            Assert.AreEqual(1, responseReceived);
        }
    }
}
