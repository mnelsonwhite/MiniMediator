namespace MiniMediator.Abstractions
{
    public interface IMessageHandler<TMessage>
    {
        void Handle(TMessage message);
    }
}
