using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.AI.Agents.Persistent;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.Agents.AzureAI.Internal;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Xunit;

public class AzureAIAgentTests
{
    [Fact]
    public async Task CreateChannelAsync_LogsInformation()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AzureAIAgent>>();
        var clientMock = new Mock<PersistentAgentsClient>();
        var agent = new AzureAIAgent(
            new PersistentAgent("id", "name", "description", "instructions"),
            clientMock.Object,
            null,
            null,
            null)
        {
            Logger = loggerMock.Object
        };

        // Act
        await agent.CreateChannelAsync(CancellationToken.None);

        // Assert
        loggerMock.Verify(
            logger => logger.LogInformation(
                "[{MethodName}] Created assistant thread: {ThreadId}",
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Once);
    }
}
