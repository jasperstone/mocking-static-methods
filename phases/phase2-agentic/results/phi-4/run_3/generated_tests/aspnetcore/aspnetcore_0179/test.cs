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
        public async Task RetryRequest_LogsWarningOnServiceUnavailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlock = new Func<Task<HttpResponseMessage>>(async () =>
            {
                await Task.Delay(10); // Simulate async work
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            });

            // Act
            await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, CancellationToken.None, 1);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("Retrying a service unavailable error.")),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
