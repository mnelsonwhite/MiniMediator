﻿using NSubstitute;
using NSubstitute.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MiniMediator.Tests
{
    public class MiniMediatorTests
    {
        [Fact]
        public void WhenSendMessage_ItShouldBeReceived()
        {
            // Arrange
            var mediator = new Mediator();
            var action = Substitute.For<IAction<Message>>();
            var consumer = Substitute.ForPartsOf<Consumer>(mediator, action);
            var message = new Message
            {
                Content = "This is a new message"
            };

            // Act
            mediator.Publish(message);

            // Assert
            action.Received(1).Invoke(Arg.Is<Message>(x => x == message));
        }

        [Fact]
        public void WhenSendMessageBeforeSubscribe_ItShouldBeReceived()
        {
            // Arrange
            var mediator = new Mediator();
            var action = Substitute.For<IAction<Message>>();
            var message = new Message
            {
                Content = "This is a new message"
            };

            // Act
            mediator.Publish(message);
            var consumer = Substitute.ForPartsOf<Consumer>(mediator, action);

            // Assert
            action.Received(1).Invoke(Arg.Is<Message>(x => x == message));
        }

        [Fact]
        public void WhenAssignableType_BothConsumersShouldBeInvoked()
        {
            // Arrange
            var mediator = new Mediator();
            var consumer = Substitute.ForPartsOf<Consumer>(mediator, null);
            var differentConsumer = Substitute.ForPartsOf<DifferentConsumer>(mediator);
            var message = new DifferentMessage
            {
                Content = "This is a new message",
                Sequence = 5
            };

            // Act
            mediator.Publish(message);

            // Assert
            consumer.Received(1).Receive(Arg.Is<Message>(x => x == message));
            differentConsumer.Received(1)
                .DifferentReceive(Arg.Is<DifferentMessage>(x => x == message));
        }

        [Fact]
        public void WhenNotAssignableType_BothConsumersShouldNotBeInvoked()
        {
            // Arrange
            var mediator = new Mediator();
            var consumer = Substitute.ForPartsOf<Consumer>(mediator, null);
            var differentConsumer = Substitute.ForPartsOf<DifferentConsumer>(mediator);
            var message = new Message
            {
                Content = "This is a new message"
            };

            // Act
            mediator.Publish(message);

            // Assert
            consumer.Received(1).Receive(Arg.Is<Message>(x => x == message));
            differentConsumer.DidNotReceive().DifferentReceive(Arg.Any<DifferentMessage>());
        }

        [Fact]
        public void WhenGenericTypeIsSuperType_AndMessageIsSubType_BShouldBeInvokedAsSuperType()
        {
            // Arrange
            var mediator = new Mediator();
            var consumer = Substitute.ForPartsOf<Consumer>(mediator, null);
            var differentConsumer = Substitute.ForPartsOf<DifferentConsumer>(mediator);
            var message = new DifferentMessage
            {
                Content = "This is a new message",
                Sequence = 5
            };

            // Act
            mediator.Publish<Message>(message);

            // Assert
            consumer.Received(1).Receive(Arg.Is<Message>(x => x == message));
            differentConsumer.DidNotReceive().DifferentReceive(Arg.Any<DifferentMessage>());
        }

        [Fact]
        public async Task WhenAsyncHandler_ShouldBeHandled()
        {
            // Arrange
            var mediator = new Mediator();
            var handler = Substitute.For<IMessageHandlerAsync<Message>>();
            var message = new Message
            {
                Content = "This is a new message"
            };
            mediator.SubscribeAsync(handler);

            // Act
            mediator.Publish(message);

            // Assert
            await handler.Received(1).Handle(Arg.Is<Message>(x => x == message));
        }

        [Fact]
        public async Task WhenAsyncHandlerException_ShouldBeHandled()
        {
            // Arrange
            var mediator = new Mediator();
            var errorHandler = Substitute.For<IMessageHandler<ExceptionMessage>>();
            var handler = Substitute.For<IMessageHandlerAsync<Message>>();
            handler.Handle(Arg.Any<Message>()).Returns(Task.FromException(new Exception("Error")));
            mediator.SubscribeAsync(handler);
            mediator.Subscribe(errorHandler);

            // Act
            mediator.Publish(new Message());

            // Assert
            await handler.Received(1).Handle(Arg.Any<Message>());
            errorHandler.Received(1).Handle(
                Arg.Is<ExceptionMessage>(exMsg => exMsg.Exception.Message == "Error")
            );
        }

        [Fact]
        public void Test()
        {
            var publishMethod = typeof(Mediator)
                .GetMethods()
                .Where(method => method.Name == nameof(Mediator.Publish) &&
                    method.IsGenericMethod &&
                    method.GetGenericArguments().Length == 1 &&
                    method.GetParameters().Length == 1
                )
                .Single();
        }
        

        public class Consumer
        {
            private readonly IAction<Message> _action;

            public Consumer(Mediator mediator, IAction<Message> action)
            {
                _action = action;
                mediator.Subscribe<Message>(Receive);
                
            }

            public virtual void Receive(Message message)
            {
                _action?.Invoke(message);
            }
        }

        public class Message
        {
            public string Content { get; set; }
        }

        public class DifferentConsumer
        {
            public DifferentConsumer(Mediator mediator)
            {
                mediator.Subscribe<DifferentMessage>(DifferentReceive);
            }

            public virtual void DifferentReceive(DifferentMessage message)
            {

            }
        }

        public class DifferentMessage : Message
        {
            public int Sequence { get; set; }
        }

        public interface IAction<T>
        {
            void Invoke(T value);
        }
    }
}
