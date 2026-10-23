using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Amazon.Core;

namespace Microsoft.SemanticKernel.Connectors.Amazon.Tests
{
    public class BedrockChatCompletionClientTests
    {
        [Fact]
        public async Task StreamChatMessageAsync_LogsError_WhenConverseStreamFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var bedrockChatCompletionClient = new BedrockChatCompletionClient("modelId", null, new LoggerFactory());
            bedrockChatCompletionClient._logger = loggerMock.Object;

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => bedrockChatCompletionClient.StreamChatMessageAsync(null, null, null));
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Can't converse stream with '{ModelId}'. Reason: {Error}", "modelId", It.IsAny<string>()), Times.Once);
        }
    }
}
