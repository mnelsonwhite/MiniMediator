using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using Xunit;

namespace MiniMediator.DependencyInjection.Tests
{
    public class ContainerExtensionsTests
    {
        [Fact]
        public void Test()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(provider => Substitute.For<IAction<TestMessage>>());
            serviceCollection.AddMediator(config => config.Assemblies.Add(GetType().Assembly));
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            serviceProvider.GetService<Mediator>().Publish(new TestMessage());

            // Assert
            serviceProvider.GetService<IAction<TestMessage>>().Received(1).Invoke(Arg.Any<TestMessage>());
        }
    }

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

    public class TestMessage
    {

    }

    public interface IAction<T>
    {
        void Invoke(T value);
    }
}
