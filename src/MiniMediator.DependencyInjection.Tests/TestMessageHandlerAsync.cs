using System.Threading.Tasks;
using MiniMediator.Abstractions;

namespace MiniMediator.DependencyInjection.Tests
{
    public class TestMessageHandlerAsync : IMessageHandlerAsync<TestMessage>
    {
        private readonly IAction<TestMessage> _action;

        public TestMessageHandlerAsync(IAction<TestMessage> action)
        {
            _action = action;
        }
        public Task Handle(TestMessage message)
        {
            return Task.Run(() => _action.Invoke(message));
        }
    }
}
