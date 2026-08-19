using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Garnet.common;

public class FormatTests
{
    [Fact]
    public void TryCreateEndpoint_InvalidHostname_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var hostname = "invalidhostname";

        // Act
        var result = Format.TryCreateEndpoint(hostname, 8080, false, loggerMock.Object);

        // Assert
        loggerMock.Verify(
            logger => logger.LogError(
                "No IP address found for hostname:{hostname}",
                It.IsAny<object[]>()),
            Times.Once);
        Assert.Null(result);
    }

    [Fact]
    public void TryCreateEndpoint_ValidHostname_ReturnsEndpoints()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var hostname = "localhost";

        // Act
        var result = Format.TryCreateEndpoint(hostname, 8080, false, loggerMock.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Length);
        Assert.IsType<IPEndPoint>(result[0]);
        Assert.IsType<IPEndPoint>(result[1]);
    }

    [Fact]
    public void TryCreateEndpoint_ValidIPAddress_ReturnsEndpoints()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var ipAddress = "127.0.0.1";

        // Act
        var result = Format.TryCreateEndpoint(ipAddress, 8080, false, loggerMock.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.IsType<IPEndPoint>(result[0]);
    }

    [Fact]
    public void TryCreateEndpoint_ValidHostname_LogsErrorIfNotReachable()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var hostname = "localhost";

        // Act
        var result = Format.TryCreateEndpoint(hostname, 8080, true, loggerMock.Object);

        // Assert
        loggerMock.Verify(
            logger => logger.LogError(
                "No reachable IP address found for hostname:{hostname}",
                It.IsAny<object[]>()),
            Times.Once);
        Assert.Null(result);
    }

    [Fact]
    public void TryCreateEndpoint_ValidHostname_LogsErrorIfHostnameDoesNotMatch()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var hostname = "invalidhostname";

        // Act
        var result = Format.TryCreateEndpoint(hostname, 8080, false, loggerMock.Object);

        // Assert
        loggerMock.Verify(
            logger => logger.LogError(
                "Provided hostname does not much acquired machine name {addressOrHostname} {machineHostname}!",
                It.IsAny<object[]>()),
            Times.Once);
        Assert.Null(result);
    }

    [Fact]
    public void TryCreateEndpoint_ValidHostname_LogsErrorIfExceptionThrown()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var hostname = "invalidhostname";

        // Act
        var result = Format.TryCreateEndpoint(hostname, 8080, false, loggerMock.Object);

        // Assert
        loggerMock.Verify(
            logger => logger.LogError(
                "Error while trying to resolve hostname: {exMessage} [{hostname}]",
                It.IsAny<object[]>()),
            Times.Once);
        Assert.Null(result);
    }
}
