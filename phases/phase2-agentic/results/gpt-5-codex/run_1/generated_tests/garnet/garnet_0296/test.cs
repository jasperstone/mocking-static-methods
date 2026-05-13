using System;
using System.Linq;
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
        public void TryCreateEndpoint_MismatchedHostname_LogsErrorAndReturnsNull()
        {
            const string providedHostname = "ProvidedHost";
            const string machineHostname = "ActualMachine";
            var ipAddresses = new[]
            {
                IPAddress.Parse("192.168.1.10"),
                IPAddress.Parse("192.168.1.11")
            };

            var loggerMock = new Mock<ILogger>(MockBehavior.Loose);

            var dnsGetHostAddresses = typeof(Dns).GetMethod("GetHostAddresses", new[] { typeof(string) });
            var getHostName = typeof(Format).GetMethod("GetHostName", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            Assert.NotNull(dnsGetHostAddresses);
            Assert.NotNull(getHostName);

            using (ShimsContext.Create())
            {
                System.Net.Fakes.ShimDns.GetHostAddressesString = _ => ipAddresses;
                System.Fakes.ShimEnvironment.MachineNameGet = () => machineHostname;

                var result = Format.TryCreateEndpoint(providedHostname, 8080, tryConnect: false, logger: loggerMock.Object);

                Assert.Null(result);
            }

            loggerMock.Verify(
                log => log.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) =>
                        state.ToString() == "Provided hostname does not much acquired machine name {addressOrHostname} {machineHostname}!"),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
