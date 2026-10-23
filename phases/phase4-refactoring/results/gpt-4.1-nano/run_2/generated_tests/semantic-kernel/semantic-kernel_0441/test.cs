using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Connectors.Chroma;

namespace Chroma.UnitTests
{
    public class ChromaClientTests
    {
        [Fact]
        public async Task ListCollectionsAsync_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ChromaClient>>();
            var httpClient = new HttpClient(); // Not used for this test, but required for constructor
            var client = new ChromaClient(httpClient, "http://localhost");
            // Inject the mocked logger
            typeof(ChromaClient).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(client, loggerMock.Object);

            // Setup the HTTP response to return a dummy collection list
            var dummyResponseContent = "[{\"Name\": \"TestCollection\"}]";

            // Mock the ExecuteHttpRequestAsync method to return the dummy response
            var executeMethod = typeof(ChromaClient).GetMethod("ExecuteHttpRequestAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = Task.FromResult<(HttpResponseMessage, string)>((new HttpResponseMessage(System.Net.HttpStatusCode.OK), dummyResponseContent));
            var mockChromaClient = new Mock<ChromaClient>(httpClient, "http://localhost");
            mockChromaClient.CallBase = true;
            mockChromaClient
                .Setup(c => c.ExecuteHttpRequestAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Returns(task);

            // Act
            await mockChromaClient.Object.ListCollectionsAsync();

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
