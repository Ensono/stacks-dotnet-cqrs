using System;
using System.Collections.Generic;
using Amido.Stacks.Application.CQRS.ApplicationEvents;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using xxAMIDOxx.xxSTACKSxx.Application.CQRS.Events;

namespace xxAMIDOxx.xxSTACKSxx.Worker.UnitTests;

[Trait("TestType", "UnitTests")]
public class ChangeFeedListenerTests
{
    private readonly IApplicationEventPublisher appEventPublisher;
    private readonly ILogger<ChangeFeedListener> logger;

    public ChangeFeedListenerTests()
    {
        appEventPublisher = Substitute.For<IApplicationEventPublisher>();
        logger = Substitute.For<ILogger<ChangeFeedListener>>();
    }

    [Fact]
    public void TestExecution()
    {
        var changeFeedListener = new ChangeFeedListener(appEventPublisher, logger);

        var trigger = GetDocuments(3);

        changeFeedListener.Run(trigger);

        appEventPublisher.Received(3).PublishAsync(Arg.Any<IApplicationEvent>());
    }

    [Fact]
    public void TestExecution_EmptyList()
    {
        var changeFeedListener = new ChangeFeedListener(appEventPublisher, logger);

        var trigger = GetDocuments(0);

        changeFeedListener.Run(trigger);

        appEventPublisher.Received(0).PublishAsync(Arg.Any<IApplicationEvent>());
    }

    private IReadOnlyList<CosmosDbChangeFeedEvent> GetDocuments(int quantity)
    {
        var result = new List<CosmosDbChangeFeedEvent>();

        for (int i = 0; i < quantity; i++)
        {
            var id = Guid.NewGuid();
            result.Add(new CosmosDbChangeFeedEvent(
                operationCode: 101,
                correlationId: id,
                entityId: id,
                eTag: Guid.NewGuid().ToString()));
        }

        return result;
    }
}
