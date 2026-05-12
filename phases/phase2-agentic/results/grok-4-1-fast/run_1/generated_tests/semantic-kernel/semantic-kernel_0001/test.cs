using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.AI.Agents.Persistent;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Agents.AzureAI.UnitTests;

public class AzureAIAgentTests
{
    [Fact]
    public async Task CreateChannelAsync_LogsInformation_WithExpectedMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<AzureAIAgent>>();
        var mockClient = new Mock<PersistentAgentsClient>();
        var mockAgentThreadActions = new Mock<IAgentThreadActions>(MockBehavior.Strict);

        mockClient.Setup(c => c.Threads).Returns(new Mock<IAgentThreads>(MockBehavior.Strict).Object);
        var mockThreads = mockClient.Object.Threads as Mock<IAgentThreads>;
        mockThreads!.Setup(t => t.CreateThreadAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync("test-thread-id");

        var agent = new AzureAIAgentTestFixture(mockLogger.Object, mockClient.Object);

        // Act
        var channel = await agent.CreateChannelAsync(CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("[CreateChannelAsync] Created assistant thread: test-thread-id")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        Assert.NotNull(channel);
        Assert.Equal("test-thread-id", channel.ThreadId);
    }

    private class AzureAIAgentTestFixture : AzureAIAgent
    {
        public AzureAIAgentTestFixture(ILogger<AzureAIAgent> logger, PersistentAgentsClient client)
            : base(
                new PersistentAgent { Id = "test-agent", Name = "Test", Instructions = "test" },
                client)
        {
            this._logger = logger;
        }

        private readonly ILogger<AzureAIAgent> _logger;
        protected override ILogger<AzureAIAgent> Logger => this._logger;
    }
}
