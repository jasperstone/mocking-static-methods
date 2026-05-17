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
        public async Task RetryRequest_LogsWarningOnRetry()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlock = new Func<Task<HttpResponseMessage>>(async () =>
            {
                await Task.Delay(10); // Simulate some delay
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

            // Act
            await Microsoft.AspNetCore.Server.IntegrationTesting.RetryHelper.RetryRequest(
                retryBlock, 
                loggerMock.Object, 
                new CancellationToken(), 
                retryCount: 3);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("Retry count")),
                    It.Is<object[]>(o => o.Length == 1 && o[0].ToString() == "1")),
                Times.Once);
        }
    }
}
