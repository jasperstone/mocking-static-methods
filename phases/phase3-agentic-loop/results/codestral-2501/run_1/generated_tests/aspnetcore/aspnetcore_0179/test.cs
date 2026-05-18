using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class RetryHelperTests
{
    [Fact]
    public async Task RetryRequest_LogsWarningOnServiceUnavailable()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var retryBlock = new Func<Task<HttpResponseMessage>>(() =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)));

        // Act
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            RetryHelper.RetryRequest(retryBlock, mockLogger.Object, CancellationToken.None, 1));

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retrying a service unavailable error.")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }
}
