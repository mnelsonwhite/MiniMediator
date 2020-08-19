using System.Threading.Tasks;

namespace MiniMediator.Abstractions
{
    public interface IMessageHandlerAsync<TMessage>
    {
        Task Handle(TMessage message);
    }
}
