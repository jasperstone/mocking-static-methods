using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class RetryHelperTests
    {
        [Fact]
        public async Task RetryRequest_LogsWarningOnHttpRequestException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            Func<Task<HttpResponseMessage>> retryBlock = () => throw new HttpRequestException("Connection failed");

            // Act
            await Assert.ThrowsAsync<Exception>(() => 
                RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 1));

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to complete the request : Connection failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RetryRequest_LogsWarningOnWebException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            Func<Task<HttpResponseMessage>> retryBlock = () => throw new WebException("Web error");

            // Act
            await Assert.ThrowsAsync<Exception>(() => 
                RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 1));

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to complete the request : Web error")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RetryRequest_DoesNotLogSpecificWarningOnOtherException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            Func<Task<HttpResponseMessage>> retryBlock = () => throw new InvalidOperationException("Invalid operation");

            // Act
            await Assert.ThrowsAsync<Exception>(() => 
                RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 1));

            // Assert - No specific warning for non-HttpRequestException/WebException
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to complete the request")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public async Task RetryRequest_LogsRetryCountWarningOnFirstAttempt()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            Func<Task<HttpResponseMessage>> retryBlock = () => throw new HttpRequestException("Test");

            // Act
            await Assert.ThrowsAsync<Exception>(() => 
                RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 1));

            // Assert - Logs retry count warning on first attempt
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retry count 1..")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
