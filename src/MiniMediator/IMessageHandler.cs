namespace MiniMediator
{
    public interface IMessageHandler<TMessage>
    {
        void Handle(TMessage message);
    }
}
