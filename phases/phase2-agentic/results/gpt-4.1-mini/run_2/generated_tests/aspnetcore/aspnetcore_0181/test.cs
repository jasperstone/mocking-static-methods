using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class RetryHelperTests
    {
        [Fact]
        public async Task RetryRequest_LogsWarningOnEachRetryCount()
        {
            var loggerMock = new Mock<ILogger>();
            int callCount = 0;

            // Setup retryBlock to succeed on 3rd call
            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                callCount++;
                if (callCount < 3)
                {
                    var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                    return Task.FromResult(response);
                }
                else
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    return Task.FromResult(response);
                }
            };

            var cancellationToken = CancellationToken.None;

            var response = await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, cancellationToken, retryCount: 5);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Verify LogWarning was called with "Retry count {retryCount}.." at least twice (for retries 1 and 2)
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retry count")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeast(2));
        }

        [Fact]
        public async Task RetryRequest_LogsWarningOnFailedRequestException()
        {
            var loggerMock = new Mock<ILogger>();
            int callCount = 0;

            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                callCount++;
                if (callCount == 1)
                {
                    throw new HttpRequestException("Request failed");
                }
                else
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                }
            };

            var cancellationToken = CancellationToken.None;

            var response = await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, cancellationToken, retryCount: 3);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Verify LogWarning was called with the message containing "Failed to complete the request"
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to complete the request")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
