using System;
using System.Collections.Generic;
using System.Net.Http;
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

        public ChromaClientLoggingTests()
        {
            _loggerMock = new Mock<ILogger<ChromaClient>>();
        }

        private class TestChromaClient : ChromaClient
        {
            private readonly Func<(HttpResponseMessage, string)> _responseFunc;

            public TestChromaClient(ILogger<ChromaClient> logger, Func<(HttpResponseMessage, string)> responseFunc)
                : base("http://localhost")
            {
                _responseFunc = responseFunc;
                typeof(ChromaClient).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(this, logger);
            }

            protected override async Task<(HttpResponseMessage, string)> ExecuteHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return await Task.FromResult(_responseFunc());
            }
        }

        [Fact]
        public async Task ListCollectionsAsync_ShouldLogDebug()
        {
            // Arrange
            var responseContent = "[{\"Name\": \"col1\"}, {\"Name\": \"col2\"}]";
            var responseMessage = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK);
            var client = new TestChromaClient(_loggerMock.Object, () => (responseMessage, responseContent));

            // Act
            await foreach (var _ in client.ListCollectionsAsync())
            {
                // consume the async enumerable
            }

            // Assert
            _loggerMock.Verify(
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
