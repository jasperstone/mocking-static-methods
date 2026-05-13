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
        public async Task RetryRequest_LogsWarningOnHttpRequestException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlock = new Func<Task<HttpResponseMessage>>(async () =>
            {
                throw new HttpRequestException("Test exception");
            });

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                RetryHelper.RetryRequest(retryBlock, loggerMock.Object, new CancellationToken(), retryCount: 3));

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("Failed to complete the request")),
                    It.Is<object[]>(o => o.Length == 1 && o[0].ToString() == "Test exception")),
                Times.Exactly(2)); // Called twice for two retries
        }
    }
}
