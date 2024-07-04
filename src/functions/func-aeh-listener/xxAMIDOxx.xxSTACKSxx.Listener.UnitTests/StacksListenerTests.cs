using System;
using System.Collections.Generic;
using System.Text;
using Amido.Stacks.Messaging.Azure.EventHub.Serializers;
using Azure.Messaging.EventHubs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;
using xxAMIDOxx.xxSTACKSxx.Application.CQRS.Events;

namespace xxAMIDOxx.xxSTACKSxx.Listener.UnitTests;

[Trait("TestType", "UnitTests")]
public class StacksListenerTests
{
    private readonly IMessageReader msgReader;
    private readonly ILogger<StacksListener> logger;

    public StacksListenerTests()
    {
        msgReader = Substitute.For<IMessageReader>();
        logger = Substitute.For<ILogger<StacksListener>>();
    }

    [Fact]
    public void TestExecution()
    {
        var eventData = new List<EventData>();

        var msgBody = BuildMessageBody();
        var message = BuildEventData(msgBody);

        eventData.Add(message);

        var stacksListener = new StacksListener(msgReader, logger);

        stacksListener.Run(eventData.ToArray());

        msgReader.Received(1).Read<MenuCreatedEvent>(message);
    }

    public MenuCreatedEvent BuildMessageBody()
    {
        var id = Guid.NewGuid();
        return new MenuCreatedEvent(new TestOperationContext(), id);
    }

    public EventData BuildEventData(MenuCreatedEvent body)
    {
        var byteArray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));

        var eventData = new EventData(byteArray);

        return eventData;
    }
}
