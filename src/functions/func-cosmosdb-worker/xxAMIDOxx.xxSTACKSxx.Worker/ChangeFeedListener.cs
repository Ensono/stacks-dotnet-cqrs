using System;
using System.Collections.Generic;
using Amido.Stacks.Application.CQRS.ApplicationEvents;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using xxAMIDOxx.xxSTACKSxx.Application.CQRS.Events;

namespace xxAMIDOxx.xxSTACKSxx.Worker;

public class ChangeFeedListener
{
    private readonly IApplicationEventPublisher appEventPublisher;
    private readonly ILogger<ChangeFeedListener> logger;

    public ChangeFeedListener(
        IApplicationEventPublisher appEventPublisher,
        ILogger<ChangeFeedListener> logger)
    {
        this.appEventPublisher = appEventPublisher;
        this.logger = logger;
    }

    [FunctionName(Constants.FunctionNames.CosmosDbChangeFeedListener)]
    public void Run([CosmosDBTrigger(
        databaseName: "%COSMOSDB_DATABASE_NAME%",
        containerName: "%COSMOSDB_COLLECTION_NAME%",
        Connection  = "COSMOSDB_CONNECTIONSTRING",
        LeaseContainerName  = "%COSMOSDB_LEASE_COLLECTION_NAME%",
        CreateLeaseContainerIfNotExists  = true)]IReadOnlyList<CosmosDbChangeFeedEvent> input)
    {
        if (input != null && input.Count > 0)
        {
            logger.LogInformation("Documents modified " + input.Count);
            foreach (var changedItem in input)
            {
                logger.LogInformation("Document read. Id: " + changedItem.EntityId);

                var cosmosDbEvent = new CosmosDbChangeFeedEvent(
                    operationCode: 999,
                    correlationId: Guid.NewGuid(),
                    entityId: changedItem.EntityId,
                    eTag: changedItem.ETag);

                appEventPublisher.PublishAsync(cosmosDbEvent);
            }
        }
    }
}
