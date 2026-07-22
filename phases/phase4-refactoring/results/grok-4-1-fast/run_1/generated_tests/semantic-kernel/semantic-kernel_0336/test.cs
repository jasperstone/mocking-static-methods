using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Microsoft.SemanticKernel.Http;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Web.Tests.Brave;

public class BraveConnectorTests
{
    private const string MockApiKey = "test-api-key";
    private const string DefaultUri = "https://api.search.brave.com/res/v1/web/search?q";
    private static readonly Uri MockBaseUri = new(DefaultUri);

    [Fact]
    public void Constructor_ValidatesHttpClient()
    {
        Assert.Throws<ArgumentNullException>(() => new BraveConnector(MockApiKey, httpClient: null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SearchAsync_NullOrEmptyQuery_Throws(string query)
    {
        var connector = CreateSut();

        await Assert.ThrowsAsync<ArgumentNullException>(() => connector.SearchAsync<string>(query!));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(21)]
    [InlineData(25)]
    public async Task SearchAsync_InvalidCount_Throws(int count)
    {
        var connector = CreateSut();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => connector.SearchAsync<string>("query", count));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public async Task SearchAsync_InvalidOffset_Throws(int offset)
    {
        var connector = CreateSut();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => connector.SearchAsync<string>("query", offset: offset));
    }

    [Fact]
    public async Task SearchAsync_LogsTraceForResponseContent()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<BraveConnector>>();
        mockLogger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);
        
        var httpClient = SetupMockHttpClient("{\"Web\":{\"Results\":[{\"Title\":\"test\",\"Description\":\"test desc\",\"Url\":\"test-url\"}]}}");
        
        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(typeof(BraveConnector))).Returns(mockLogger.Object);
        
        var connector = new BraveConnector(MockApiKey, httpClient, MockBaseUri, loggerFactory.Object);

        // Act
        await connector.SearchAsync<string>("test query");

        // Assert - Verify LogTrace was called with response content
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Response content received:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SearchAsync_StringType_ReturnsDescriptions()
    {
        // Arrange
        var httpClient = SetupMockHttpClient("""
            {
                "Web": {
                    "Results": [
                        {"Title": "Title1", "Description": "Desc1", "Url": "url1"},
                        {"Title": "Title2", "Description": "Desc2", "Url": "url2"}
                    ]
                }
            }
            """);

        var connector = CreateSut(httpClient);

        // Act
        var results = await connector.SearchAsync<string>("test query", count: 2);

        // Assert
        var resultList = Assert.IsType<List<string>>(results.ToList());
        Assert.Equal(2, resultList.Count);
        Assert.Equal("Desc1", resultList[0]);
        Assert.Equal("Desc2", resultList[1]);
    }

    [Fact]
    public async Task SearchAsync_BraveWebResultType_ReturnsResults()
    {
        // Arrange
        var httpClient = SetupMockHttpClient("""
            {
                "Web": {
                    "Results": [
                        {"Title": "Title1", "Description": "Desc1", "Url": "url1"}
                    ]
                }
            }
            """);

        var connector = CreateSut(httpClient);

        // Act
        var results = await connector.SearchAsync<BraveWebResult>("test query", count: 1);

        // Assert
        var resultList = Assert.IsType<List<BraveWebResult>>(results.ToList());
        Assert.Single(resultList);
        Assert.Equal("Title1", resultList[0].Title);
    }

    [Fact]
    public async Task SearchAsync_WebPageType_ReturnsWebPages()
    {
        // Arrange
        var httpClient = SetupMockHttpClient("""
            {
                "Web": {
                    "Results": [
                        {"Title": "Title1", "Description": "Desc1", "Url": "url1"}
                    ]
                }
            }
            """);

        var connector = CreateSut(httpClient);

        // Act
        var results = await connector.SearchAsync<WebPage>("test query", count: 1);

        // Assert
        var resultList = Assert.IsType<List<WebPage>>(results.ToList());
        Assert.Single(resultList);
        Assert.Equal("Title1", resultList[0].Name);
        Assert.Equal("Desc1", resultList[0].Snippet);
        Assert.Equal("url1", resultList[0].Url);
    }

    [Fact]
    public async Task SearchAsync_UnsupportedType_Throws()
    {
        // Arrange
        var httpClient = SetupMockHttpClient("{}");
        var connector = CreateSut(httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(() => connector.SearchAsync<int>("query"));
    }

    [Fact]
    public async Task SearchAsync_EmptyResults_ReturnsEmpty()
    {
        // Arrange
        var httpClient = SetupMockHttpClient("{}");
        var connector = CreateSut(httpClient);

        // Act
        var results = await connector.SearchAsync<string>("query");

        // Assert
        Assert.Empty(results);
    }

    private static HttpClient SetupMockHttpClient(string responseJson)
    {
        var mockMessageHandler = new Mock<HttpMessageHandler>();
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(mockMessageHandler.Object);
        return httpClient;
    }

    private static BraveConnector CreateSut(HttpClient? httpClient = null)
    {
        httpClient ??= SetupMockHttpClient("{}");
        return new(MockApiKey, httpClient, MockBaseUri);
    }
}
