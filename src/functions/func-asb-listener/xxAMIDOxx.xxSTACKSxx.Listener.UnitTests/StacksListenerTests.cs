using System;
using System.Text;
using Amido.Stacks.Core.Operations;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;
using xxAMIDOxx.xxSTACKSxx.Application.CQRS.Events;

namespace xxAMIDOxx.xxSTACKSxx.Listener.UnitTests;

[Trait("TestType", "UnitTests")]
public class StacksListenerTests
{
    private readonly ILogger<StacksListener> logger;

    public StacksListenerTests()
    {
        logger = Substitute.For<ILogger<StacksListener>>();
    }

    [Fact]
    public void TestRun()
    {
        var msgBody = BuildMessageBody();
        var message = BuildMessage(msgBody);

        var stacksListener = new StacksListener(logger);

        stacksListener.Run(message);

        logger.Received(1).LogInformation($"Message read. Menu Id: {message.MessageId}");
    }

    public MenuCreatedEvent BuildMessageBody()
    {
        var id = Guid.NewGuid();
        return new MenuCreatedEvent(new TestOperationContext(), id);
    }

    public ServiceBusReceivedMessage BuildMessage(MenuCreatedEvent body)
    {
        Guid correlationId = GetCorrelationId(body);

        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            new BinaryData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body))),
            contentType: "application/json;charset=utf-8",
            correlationId: $"{correlationId}");

        return message;
    }

    private static Guid GetCorrelationId(object body)
    {
        var ctx = body as IOperationContext;
        return ctx?.CorrelationId ?? Guid.NewGuid();
    }
}
