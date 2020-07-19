using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        private readonly ServiceProvider _serviceProvider;

        public ContainerExtensionsTests()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(p => Substitute.For<ILogger>());
            serviceCollection.AddSingleton(provider => Substitute.For<IAction<TestMessage>>());
            serviceCollection.AddSingleton(provider => Substitute.For<IAction<SecondTestMessage>>());
            serviceCollection.AddMediator(config => config.Assemblies.Add(GetType().Assembly));
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        [Fact]
        public void WhenPublishMessage_ShouldBeHandled()
        {
            // Arrange
            // in ctor

            // Act
            _serviceProvider.GetService<Mediator>().Publish(new TestMessage());
            _serviceProvider.GetService<Mediator>().Publish(new SecondTestMessage());

            // Assert
            _serviceProvider.GetService<IAction<TestMessage>>().Received(1).Invoke(Arg.Any<TestMessage>());
            _serviceProvider.GetService<IAction<SecondTestMessage>>().Received(1).Invoke(Arg.Any<SecondTestMessage>());
        }
    }
}
