using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Chroma.Tests
{
    public class ChromaClientTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly HttpClient _httpClient;
        private readonly string _endpoint = "http://localhost";

        public ChromaClientTests()
        {
            _loggerMock = new Mock<ILogger>();
            _httpClient = new HttpClient(new FakeHttpMessageHandler());
        }

        [Fact]
        public async Task ListCollectionsAsync_ShouldLogDebugAndYieldCollectionNames()
        {
            // Arrange
            var client = new ChromaClient(_httpClient, _endpoint, new LoggerFactory().AddProvider(new MockLoggerProvider(_loggerMock.Object)));

            // Act
            var collections = new List<string>();
            await foreach (var name in client.ListCollectionsAsync())
            {
                collections.Add(name);
            }

            // Assert
            _loggerMock.Verify(x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Listing collections")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
            Assert.NotEmpty(collections);
        }
    }

    // Fake HttpMessageHandler to mock HTTP responses
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("[{\"Name\": \"Collection1\"}, {\"Name\": \"Collection2\"}]")
            };
            return Task.FromResult(response);
        }
    }

    // Custom LoggerProvider to inject mocked ILogger
    public class MockLoggerProvider : ILoggerProvider
    {
        private readonly ILogger _logger;

        public MockLoggerProvider(ILogger logger)
        {
            _logger = logger;
        }

        public ILogger CreateLogger(string categoryName) => _logger;

        public void Dispose() { }
    }
}
