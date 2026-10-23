using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ChromaClientTests
{
    public class ChromaClientLoggingTests
    {
        [Fact]
        public async Task ListCollectionsAsync_Should_LogDebug()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ChromaClient>>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            // Setup fake HTTP response content
            var responseContent = "[{\"Name\": \"Collection1\"}, {\"Name\": \"Collection2\"}]";
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };

            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .ReturnsAsync(responseMessage);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var client = new ChromaClient(httpClient, "http://localhost");
            // Inject the mocked logger
            typeof(ChromaClient).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(client, mockLogger.Object);

            // Act
            await foreach (var collectionName in client.ListCollectionsAsync())
            {
                // Consume the async enumerable
            }

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Listing collections")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
