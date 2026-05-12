using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Agents.AzureAI.Tests
{
    public class AzureAIAgentTests
    {
        [Fact]
        public async Task CreateChannelAsync_LogsChannelCreation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AzureAIAgent>>();
            var clientMock = new Mock<PersistentAgentsClient>();
            var agent = new AzureAIAgent(new PersistentAgent(), clientMock.Object, logger: loggerMock.Object);

            // Act
            await agent.CreateChannelAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
