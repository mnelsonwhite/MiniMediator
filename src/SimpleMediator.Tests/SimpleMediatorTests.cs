using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SimpleMediator.Tests
{
    public class SimpleMediatorTests
    {
        [Fact]
        public void WhenSendMessage_ItShouldBeReceived()
        {
            // Arrange
            var mediator = new Mediator();
            var consumer = Substitute.ForPartsOf<Consumer>(mediator);
            var message = new Message
            {
                Content = "This is a new message"
            };

            // Act
            mediator.Publish(message);

            // Assert
            consumer.Received(1).Receive(Arg.Is<Message>(x => x == message));
        }

        [Fact]
        public void WhenAssignableType_BothConsumersShouldBeInvoked()
        {
            // Arrange
            var mediator = new Mediator();
            var consumer = Substitute.ForPartsOf<Consumer>(mediator);
            var differentConsumer = Substitute.ForPartsOf<DifferentConsumer>(mediator);
            var message = new DifferentMessage
            {
                Content = "This is a new message",
                Sequence = 5
            };

            // Act
            mediator.Publish(message);

            // Assert
            consumer.Received(1).Receive(Arg.Is<Message>(x => x.Content == message.Content));
            differentConsumer.Received(1).DifferentReceive(Arg.Is<DifferentMessage>(
                x => x.Content == message.Content && x.Sequence == message.Sequence
            ));
        }

        [Fact]
        public void WhenNotAssignableType_BothConsumersShouldNotBeInvoked()
        {
            // Arrange
            var mediator = new Mediator();
            var consumer = Substitute.ForPartsOf<Consumer>(mediator);
            var differentConsumer = Substitute.ForPartsOf<DifferentConsumer>(mediator);
            var message = new Message
            {
                Content = "This is a new message"
            };

            // Act
            mediator.Publish(message);

            // Assert
            consumer.Received(1).Receive(Arg.Is<Message>(x => x.Content == message.Content));
            differentConsumer.DidNotReceive().DifferentReceive(Arg.Any<DifferentMessage>());
        }

        [Fact]
        public void WhenGenericTypeIsSuperType_AndMessageIsSubType_BShouldBeInvokedAsSuperType()
        {
            // Arrange
            var mediator = new Mediator();
            var consumer = Substitute.ForPartsOf<Consumer>(mediator);
            var differentConsumer = Substitute.ForPartsOf<DifferentConsumer>(mediator);
            var message = new DifferentMessage
            {
                Content = "This is a new message",
                Sequence = 5
            };

            // Act
            mediator.Publish<Message>(message);

            // Assert
            consumer.Received(1).Receive(Arg.Is<Message>(x => x.Content == message.Content));
            differentConsumer.DidNotReceive().DifferentReceive(Arg.Any<DifferentMessage>());
        }
        

        public class Consumer
        {
            public Consumer(Mediator mediator)
            {
                mediator.Subscribe<Message>(Receive);
            }

            public virtual void Receive(Message message)
            {

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
    }
}
