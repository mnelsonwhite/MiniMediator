using System.Threading.Tasks;

namespace MiniMediator
{
    public interface IMessageHandlerAsync<TMessage>
    {
        Task Handle(TMessage message);
    }
}
