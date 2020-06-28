namespace MiniMediator.DependencyInjection.Tests
{
    public interface IAction<T>
    {
        void Invoke(T value);
    }
}
