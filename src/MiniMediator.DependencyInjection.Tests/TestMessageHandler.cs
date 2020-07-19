namespace MiniMediator.DependencyInjection.Tests
{
    public class TestMessageHandler : IMessageHandler<TestMessage>, IMessageHandler<SecondTestMessage>
    {
        private readonly IAction<TestMessage> _action;
        private readonly IAction<SecondTestMessage> _secondAction;

        public TestMessageHandler(IAction<TestMessage> action, IAction<SecondTestMessage> secondAction)
        {
            _action = action;
            _secondAction = secondAction;
        }
        public void Handle(TestMessage message)
        {
            _action.Invoke(message);
        }

        public void Handle(SecondTestMessage message)
        {
            _secondAction.Invoke(message);
        }
    }
}
