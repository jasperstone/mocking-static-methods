using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.common.Tests
{
    public class FormatTests
    {
        [Fact]
        public async Task TryCreateEndpoint_LogsError_WhenHostnameDoesNotMatchMachineName()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            string testHostname = "testhostname";
            string machineHostname = "machinename";

            // Mock GetHostName to return a different hostname
            var format = new Format();
            format.GetHostName = () => machineHostname;

            // Act
            var result = await format.TryCreateEndpointAsync(testHostname, 8080, false, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.Is<string>(s => s.Contains("Provided hostname does not much acquired machine name")),
                    It.Is<object[]>(o => o[0].ToString() == testHostname && o[1].ToString() == machineHostname),
                    It.IsAny<Exception>()
                ),
                Times.Once
            );

            Assert.Null(result);
        }
    }
}
