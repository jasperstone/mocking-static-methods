using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Net.Sockets;
using Garnet.common;

public class FormatTests
{
    [Fact]
    public void TryCreateEndpoint_InvalidHostname_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var invalidHostname = "invalidhostname";

        // Act
        var result = Format.TryCreateEndpoint(invalidHostname, 8080, false, loggerMock.Object);

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No IP address found for hostname:invalidhostname")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
        Assert.Null(result);
    }

    [Fact]
    public void TryCreateEndpoint_ValidHostname_ReturnsEndpoints()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var validHostname = "localhost";

        // Act
        var result = Format.TryCreateEndpoint(validHostname, 8080, false, loggerMock.Object);

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
        var validIPAddress = "127.0.0.1";

        // Act
        var result = Format.TryCreateEndpoint(validIPAddress, 8080, false, loggerMock.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.IsType<IPEndPoint>(result[0]);
    }

    [Fact]
    public void TryCreateEndpoint_InvalidHostname_LogsError_WhenTryConnectIsTrue()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var invalidHostname = "invalidhostname";

        // Act
        var result = Format.TryCreateEndpoint(invalidHostname, 8080, true, loggerMock.Object);

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No IP address found for hostname:invalidhostname")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
        Assert.Null(result);
    }

    [Fact]
    public void TryCreateEndpoint_ValidHostname_LogsError_WhenHostnameDoesNotMatchMachineHostname()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var validHostname = "validhostname";

        // Act
        var result = Format.TryCreateEndpoint(validHostname, 8080, false, loggerMock.Object);

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Provided hostname does not much acquired machine name validhostname")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
        Assert.Null(result);
    }
}
