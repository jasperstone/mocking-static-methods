using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Moq;
using Xunit;
using Azure.AI.Agents.Persistent;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Microsoft.SemanticKernel.Agents.AzureAI.Tests
{
    public class AzureAIAgentTests
    {
        [Fact]
        public async Task CreateChannelAsync_LogsInformationWithMethodNameAndThreadId()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<AzureAIAgent>>();
            var mockClient = new Mock<PersistentAgentsClient>();
            var persistentAgent = new PersistentAgent
            {
                Id = "agentId",
                Name = "agentName",
                Description = "desc",
                Instructions = "instructions"
            };

            // Setup CreateThreadAsync to return a fixed threadId
            var threadId = "thread123";
            mockClient.Setup(c => c.Threads.CreateThreadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(threadId);

            // Setup Client property to return mockClient.Object
            mockClient.SetupGet(c => c.Threads).Returns(new Mock<IPersistentAgentThreads>().Object);

            // We need to mock AgentThreadActions.CreateThreadAsync static method used in CreateChannelAsync
            // Since it's static, we cannot mock it directly here.
            // Instead, we will create a derived class to override CreateChannelAsync and call base method with a mock.

            var agent = new TestAzureAIAgent(persistentAgent, mockClient.Object, mockLogger.Object, threadId);

            // Act
            var channel = await agent.CallCreateChannelAsync(CancellationToken.None);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("[CreateChannelAsync] Created assistant thread: thread123")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private class TestAzureAIAgent : AzureAIAgent
        {
            private readonly ILogger<AzureAIAgent> _logger;
            private readonly string _threadId;

            public TestAzureAIAgent(PersistentAgent model, PersistentAgentsClient client, ILogger<AzureAIAgent> logger, string threadId)
                : base(model, client)
            {
                _logger = logger;
                _threadId = threadId;
                this.Logger = _logger;
            }

            public ILogger<AzureAIAgent> Logger { get; }

            public async Task<AgentChannel> CallCreateChannelAsync(CancellationToken cancellationToken)
            {
                // We override the call to AgentThreadActions.CreateThreadAsync to return the fixed threadId
                return await CreateChannelAsync(cancellationToken);
            }

            protected override async Task<string> CreateThreadAsync(CancellationToken cancellationToken)
            {
                await Task.CompletedTask;
                return _threadId;
            }
        }
    }
}
