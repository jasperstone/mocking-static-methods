using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Web.Brave.Tests
{
    public class BraveConnectorTests
    {
        private readonly Mock<ILogger<BraveConnector>> _loggerMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly BraveConnector _braveConnector;

        public BraveConnectorTests()
        {
            _loggerMock = new Mock<ILogger<BraveConnector>>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _braveConnector = new BraveConnector("apiKey", _httpClient, null, new NullLoggerFactory());
        }

        [Fact]
        public async Task SearchAsync_ValidQuery_ReturnsResults()
        {
            // Arrange
            var query = "test";
            var count = 1;
            var offset = 0;
            var responseContent = new StringContent(JsonSerializer.Serialize(new BraveSearchResponse<BraveWebResult>
            {
                Web = new BraveWeb<BraveWebResult>
                {
                    Results = new List<BraveWebResult>
                    {
                        new BraveWebResult { Description = "Test result" }
                    }
                }
            }));

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = responseContent
                });

            // Act
            var result = await _braveConnector.SearchAsync<string>(query, count, offset);

            // Assert
            Assert.Single(result);
            Assert.Equal("Test result", result.First());
        }

        [Fact]
        public async Task SearchAsync_InvalidCount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var query = "test";
            var count = 21;
            var offset = 0;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _braveConnector.SearchAsync<string>(query, count, offset));
        }

        [Fact]
        public async Task SearchAsync_InvalidOffset_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var query = "test";
            var count = 1;
            var offset = 11;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _braveConnector.SearchAsync<string>(query, count, offset));
        }

        [Fact]
        public async Task SearchAsync_LogsTrace()
        {
            // Arrange
            var query = "test";
            var count = 1;
            var offset = 0;
            var responseContent = new StringContent(JsonSerializer.Serialize(new BraveSearchResponse<BraveWebResult>
            {
                Web = new BraveWeb<BraveWebResult>
                {
                    Results = new List<BraveWebResult>
                    {
                        new BraveWebResult { Description = "Test result" }
                    }
                }
            }));

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = responseContent
                });

            // Act
            await _braveConnector.SearchAsync<string>(query, count, offset);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
