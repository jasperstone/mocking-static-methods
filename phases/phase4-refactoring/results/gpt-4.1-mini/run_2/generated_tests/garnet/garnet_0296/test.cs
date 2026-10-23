using System;
using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.common;

namespace Garnet.Tests
{
    public class FormatTests
    {
        [Fact]
        public void TryCreateEndpoint_LogsErrorWhenNoIpAddressFound()
        {
            var loggerMock = new Mock<ILogger>();
            string hostname = "nonexistent.hostname.test";

            var result = Format.TryCreateEndpoint(hostname, 1234, tryConnect: false, logger: loggerMock.Object);

            Assert.Null(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No IP address found for hostname")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_LogsErrorWhenHostnameDoesNotMatchMachineName()
        {
            var loggerMock = new Mock<ILogger>();
            string hostname = "somehostname";

            var result = Format.TryCreateEndpoint(hostname, 1234, tryConnect: false, logger: loggerMock.Object);

            Assert.Null(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Provided hostname does not much acquired machine name")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
