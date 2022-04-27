
namespace TestBehaviors.V7
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NServiceBus;
    using TestComms;
    using TestLogicApi;

    class Sender : ITestBehavior
    {
        public EndpointConfiguration Configure(Dictionary<string, string> args)
        {
            var config = new EndpointConfiguration("Sender");

            var routing = config.UseTransport<LearningTransport>().Routing();
            routing.RouteToEndpoint(typeof(MyRequest), "Receiver");

            return config;
        }

        public async Task Execute(IEndpointInstance endpointInstance)
        {
            await endpointInstance.Send(new MyRequest());
        }

        public class MyResponseHandler : IHandleMessages<MyResponse>
        {
            ITestContextAccessor contextAccessor;

            public MyResponseHandler(ITestContextAccessor contextAccessor)
            {
                this.contextAccessor = contextAccessor;
            }

            public Task Handle(MyResponse message, IMessageHandlerContext context)
            {
                contextAccessor.SetFlag("ResponseReceived", true);
                contextAccessor.Success();

                return Task.CompletedTask;
            }
        }

        
    }
}