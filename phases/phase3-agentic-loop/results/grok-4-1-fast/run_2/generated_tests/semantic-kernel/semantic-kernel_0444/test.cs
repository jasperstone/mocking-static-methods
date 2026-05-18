using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Chroma.UnitTests;

public class ChromaClientTests
{
    [Fact]
    public async Task ExecuteHttpRequestAsync_LogsError_OnHttpOperationException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ChromaClient>>();
        mockLogger.Setup(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GET") &&
                                          v.ToString()!.Contains("test-operation") &&
                                          v.ToString()!.Contains("Test error message") &&
                                          v.ToString()!.Contains("Error response content")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpOperationException = new HttpOperationException("Test error message")
        {
            ResponseContent = "Error response content"
        };

        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(httpOperationException);

        var httpClient = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("http://test-endpoint")
        };

        var loggerFactory = Mock.Of<ILoggerFactory>();
        mockLogger.Setup(lf => lf.CreateLogger(typeof(ChromaClient).FullName!)).Returns(mockLogger.Object);

        var client = new ChromaClient(httpClient, endpoint: null, loggerFactory: loggerFactory);

        var request = new HttpRequestMessage(HttpMethod.Get, "test-operation");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpOperationException>(
            () => InvokePrivateMethod(client, request));

        mockLogger.VerifyAll();
    }

    private static async Task<(HttpResponseMessage, string)> InvokePrivateMethod(ChromaClient client, HttpRequestMessage request)
    {
        var method = typeof(ChromaClient).GetMethod("ExecuteHttpRequestAsync", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var task = (Task)method.Invoke(client, new object[] { request, CancellationToken.None })!;
        await task.ConfigureAwait(false);
        throw new InvalidOperationException("Should not reach here");
    }
}
