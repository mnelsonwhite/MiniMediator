namespace MiniMediator.DependencyInjection.Tests
{
    public class TestMessageHandler : IMessageHandler<TestMessage>
    {
        private readonly IAction<TestMessage> _action;

        public TestMessageHandler(IAction<TestMessage> action)
        {
            _action = action;
        }
        public void Handle(TestMessage message)
        {
            _action.Invoke(message);
        }
    }
}
