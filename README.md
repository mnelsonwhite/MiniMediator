# SimpleMediator

A simple mediator that requires no setup. Publish a message from one class and consumer it from another.

## Usage

With an example consumer and message

``` c#
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
```

Sending a message from the mediator will invoke `Receive` on the `Consumer`.

``` c#
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
```

Creating a new sub class for the message and consumer demonstrates that Consumers of message types which are super classes of published messaged will get invoked.

``` c#
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
```

Here, both consumers get invoked when the sub class `DifferentMessage` is published.

``` c#
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
consumer.Received(1).Receive(Arg.Is<Message>(x => x == message));
differentConsumer.Received(1)
    .DifferentReceive(Arg.Is<DifferentMessage>(x => x == message));
```

When the message type is not assignable to the subscribed type the subscription is not invoked.

``` c#
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
consumer.Received(1).Receive(Arg.Is<Message>(x => x == message));
differentConsumer.DidNotReceive().DifferentReceive(Arg.Any<DifferentMessage>());
```

Or if the message is of a subtype but the generic type is the supertype, then the subtype subscriptions are not invoked.
This is to allow greater control over how the message is handled. If the producer is expected type `T` it should be handled as `T`.

``` c#
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
consumer.Received(1).Receive(Arg.Is<Message>(x => x == message));
differentConsumer.DidNotReceive().DifferentReceive(Arg.Any<DifferentMessage>());
```