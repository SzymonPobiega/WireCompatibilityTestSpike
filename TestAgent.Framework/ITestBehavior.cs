namespace TestLogicApi
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NServiceBus;

    /// <summary>
    /// Represents a behavior of an endpoint within a test suite
    /// </summary>
    public interface ITestBehavior
    {
        Task Execute(IEndpointInstance endpointInstance);
        EndpointConfiguration Configure(Dictionary<string, string> args);
    }
}