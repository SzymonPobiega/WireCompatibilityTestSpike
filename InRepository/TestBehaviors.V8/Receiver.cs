namespace TestBehaviors.V8
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NServiceBus;
    using TestComms;
    using TestLogicApi;

    class Receiver : ITestBehavior
    {
        public Task Execute(IEndpointInstance endpointInstance)
        {
            return Task.CompletedTask;
        }

        public EndpointConfiguration Configure(Dictionary<string, string> args)
        {
            var config = new EndpointConfiguration("Receiver");

            config.UseTransport<LearningTransport>();

            return config;
        }

        public class MyRequestHandler : IHandleMessages<MyRequest>
        {
            ITestContextAccessor contextAccessor;

            public MyRequestHandler(ITestContextAccessor contextAccessor)
            {
                this.contextAccessor = contextAccessor;
            }

            public Task Handle(MyRequest message, IMessageHandlerContext context)
            {
                contextAccessor.SetFlag("RequestReceived", true);
                return context.Reply(new MyResponse());
            }
        }
    }
}