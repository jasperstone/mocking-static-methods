using System;
using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.common.Tests
{
    public class FormatTests
    {
        [Fact]
        public void TryCreateEndpoint_LogsError_WhenNoIPAddressesFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var hostname = "nonexistent.hostname";

            // Act
            var result = Format.TryCreateEndpoint(hostname, 8080, logger: loggerMock.Object);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(
                x => x.LogError(
                    It.Is<string>(s => s.Contains("No IP address found for hostname:")),
                    It.Is<object[]>(o => o[0].ToString() == hostname)),
                Times.Once);
        }
    }
}
