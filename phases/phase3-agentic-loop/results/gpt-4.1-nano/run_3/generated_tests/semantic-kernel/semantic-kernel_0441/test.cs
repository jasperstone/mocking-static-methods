using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Connectors.Chroma;

namespace ChromaClientTests
{
    public class ChromaClientLoggingTests
    {
        private readonly Mock<ILogger<ChromaClient>> _loggerMock;
        private readonly ChromaClient _client;

        public ChromaClientLoggingTests()
        {
            _loggerMock = new Mock<ILogger<ChromaClient>>();
            var httpClient = new System.Net.Http.HttpClient();
            _client = new ChromaClient(httpClient, "http://localhost");
        }

        [Fact]
        public async Task ListCollectionsAsync_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ChromaClient>>();
            var client = new ChromaClient(new System.Net.Http.HttpClient(), "http://localhost");
            var logger = loggerMock.Object;

            // Act
            await client.ListCollectionsAsync();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Listing collections")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
