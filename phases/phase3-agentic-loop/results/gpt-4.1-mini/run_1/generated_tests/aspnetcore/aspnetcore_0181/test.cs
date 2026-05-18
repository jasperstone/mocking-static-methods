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
        public async Task RetryRequest_LogsWarningOnRetryCount()
        {
            var loggerMock = new Mock<ILogger>();
            int callCount = 0;

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

            // Verify LogWarning was called with message containing "Retry count"
            loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(s => s.Contains("Retry count"))),
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
                    throw new HttpRequestException("Network failure");
                }
                else
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    return Task.FromResult(response);
                }
            };

            var cancellationToken = CancellationToken.None;

            var response = await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, cancellationToken, retryCount: 3);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Verify LogWarning was called with message containing "Failed to complete the request"
            loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(s => s.Contains("Failed to complete the request"))),
                Times.Once);
        }
    }
}
