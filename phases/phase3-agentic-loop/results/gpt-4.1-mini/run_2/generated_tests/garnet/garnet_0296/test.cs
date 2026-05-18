using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.common;

namespace Garnet.Tests
{
    public class FormatTests
    {
        [Fact]
        public void TryCreateEndpoint_LogsError_WhenProvidedHostnameDoesNotMatchMachineName()
        {
            var loggerMock = new Mock<ILogger>();
            string fakeHostname = "definitely-not-the-machine-hostname";

            var result = Format.TryCreateEndpoint(fakeHostname, 1234, tryConnect: false, logger: loggerMock.Object);

            Assert.Null(result);
            loggerMock.Verify(
                x => x.LogError(
                    "Provided hostname does not much acquired machine name {addressOrHostname} {machineHostname}!",
                    fakeHostname,
                    It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_LogsError_WhenNoIpAddressFound()
        {
            var loggerMock = new Mock<ILogger>();
            string fakeHostname = "no-such-hostname-should-not-exist";

            var result = Format.TryCreateEndpoint(fakeHostname, 1234, tryConnect: false, logger: loggerMock.Object);

            Assert.Null(result);
            loggerMock.Verify(
                x => x.LogError(
                    "No IP address found for hostname:{hostname}",
                    fakeHostname),
                Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_LogsError_WhenNoReachableIpAddressFound()
        {
            var loggerMock = new Mock<ILogger>();
            string hostname = "localhost";

            var result = Format.TryCreateEndpoint(hostname, 1234, tryConnect: true, logger: loggerMock.Object);

            Assert.Null(result);
            loggerMock.Verify(
                x => x.LogError(
                    "No reachable IP address found for hostname:{hostname}",
                    hostname),
                Times.Once);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_LogsError_WhenExceptionThrown()
        {
            var loggerMock = new Mock<ILogger>();
            string fakeHostname = "invalid-hostname-that-should-throw";

            var result = await Format.TryCreateEndpointAsync(fakeHostname, 1234, tryConnect: false, logger: loggerMock.Object);

            Assert.Null(result);
            loggerMock.Verify(
                x => x.LogError(
                    It.Is<string>(s => s.StartsWith("Error while trying to resolve hostname:")),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
