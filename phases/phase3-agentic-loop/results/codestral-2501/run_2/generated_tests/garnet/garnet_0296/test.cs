using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Threading.Tasks;
using Garnet.common;

namespace Garnet.common.Tests
{
    public class FormatTests
    {
        [Fact]
        public async Task TryCreateEndpointAsync_InvalidHostname_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var invalidHostname = "invalidhostname";

            // Act
            var result = await Format.TryCreateEndpointAsync(invalidHostname, 8080, false, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.LogError(
                    "Provided hostname does not much acquired machine name {addressOrHostname} {machineHostname}!",
                    It.IsAny<object[]>()),
                Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_ValidHostname_ReturnsEndpoints()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var validHostname = "localhost";

            // Act
            var result = await Format.TryCreateEndpointAsync(validHostname, 8080, false, loggerMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_NoIPAddressFound_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var invalidHostname = "invalidhostname";

            // Act
            var result = await Format.TryCreateEndpointAsync(invalidHostname, 8080, false, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.LogError(
                    "No IP address found for hostname:{hostname}",
                    It.IsAny<object[]>()),
                Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_NoReachableIPAddressFound_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var invalidHostname = "invalidhostname";

            // Act
            var result = await Format.TryCreateEndpointAsync(invalidHostname, 8080, true, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.LogError(
                    "No reachable IP address found for hostname:{hostname}",
                    It.IsAny<object[]>()),
                Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_Exception_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var invalidHostname = "invalidhostname";

            // Act
            var result = await Format.TryCreateEndpointAsync(invalidHostname, 8080, false, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.LogError(
                    "Error while trying to resolve hostname: {exMessage} [{hostname}]",
                    It.IsAny<object[]>()),
                Times.Once);
            Assert.Null(result);
        }
    }
}
