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

namespace Microsoft.SemanticKernel.Plugins.Web.Tests.Brave;

public class BraveConnectorTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILogger<BraveConnector>> _loggerMock;

    public BraveConnectorTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _loggerMock = new Mock<ILogger<BraveConnector>>();
    }

    [Fact]
    public async Task SearchAsync_LogsTraceWithResponseContent()
    {
        // Arrange
        var apiKey = "test-api-key";
        var loggerFactory = Mock.Of<ILoggerFactory>(f => f.CreateLogger(typeof(BraveConnector)) == _loggerMock.Object);
        var connector = new BraveConnector(apiKey, _httpClient, null, loggerFactory);

        var expectedJson = "{\"Web\":{\"Results\":[{\"Title\":\"Test\",\"Description\":\"Test desc\",\"Url\":\"http://test.com\"}]}}";
        SetupHttpResponse(expectedJson);

        // Act
        await connector.SearchAsync<string>("test query");

        // Assert - Verify LogTrace was called with expected message and JSON data
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Response content received: ") && v.ToString()!.Contains(expectedJson)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private void SetupHttpResponse(string jsonResponse)
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse)
            });
    }
}
