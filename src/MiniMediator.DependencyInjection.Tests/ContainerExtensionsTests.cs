using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniMediator.Abstractions;
using NSubstitute;
using Xunit;

namespace MiniMediator.DependencyInjection.Tests
{
    public class ContainerExtensionsTests
    {
        [Fact]
        public void WhenPublishMessage_ShouldBeHandled()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(p => Substitute.For<ILogger>());
            serviceCollection.AddSingleton(provider => Substitute.For<IAction<TestMessage>>());
            serviceCollection.AddSingleton(provider => Substitute.For<IAction<SecondTestMessage>>());
            serviceCollection.AddMediator(config => config.Assemblies.Add(GetType().Assembly));
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            serviceProvider.GetService<IMediator>().Publish(new TestMessage());
            serviceProvider.GetService<IMediator>().Publish(new SecondTestMessage());

            // Assert
            serviceProvider.GetService<IAction<TestMessage>>().Received(2).Invoke(Arg.Any<TestMessage>());
            serviceProvider.GetService<IAction<SecondTestMessage>>().Received(1).Invoke(Arg.Any<SecondTestMessage>());
        }

        [Fact]
        public async Task WhenRegisteringhandlerTypes_ShouldBeHanled()
        {
            // Arrange
            var handler = Substitute.For<IMessageHandler<TestMessage>>();
            var asyncHandler = Substitute.For<IMessageHandlerAsync<TestMessage>>();
            var filteredHander = Substitute.For<IFilteredMessageHandler<TestMessage>>();
            var filteredAsyncHander = Substitute.For<IFilteredMessageHandlerAsync<TestMessage>>();
            filteredHander.Predicate.Returns(m => true);
            filteredAsyncHander.Predicate.Returns(m => true);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(p => handler);
            serviceCollection.AddSingleton(p => asyncHandler);
            serviceCollection.AddSingleton(p => filteredHander);
            serviceCollection.AddSingleton(p => filteredAsyncHander);
            serviceCollection.AddMediator();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            serviceProvider.GetService<IMediator>().Publish(new TestMessage());

            // Assert
            handler.Received(1).Handle(Arg.Any<TestMessage>());
            await asyncHandler.Received(1).Handle(Arg.Any<TestMessage>());
            filteredHander.Received(1).Handle(Arg.Any<TestMessage>());
            await filteredAsyncHander.Received(1).Handle(Arg.Any<TestMessage>());
        }
    }
}
