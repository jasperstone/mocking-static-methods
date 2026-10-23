using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Net.Sockets;
using Garnet.common;

public class FormatTests
{
    [Fact]
    public void SimpleTest()
    {
        Assert.True(true);
    }

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
            logger => logger.LogError(
                "No IP address found for hostname:{hostname}",
                It.IsAny<object[]>()
            ),
            Times.Once
        );
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
        Assert.Equal(1, result.Length);
        Assert.IsType<IPEndPoint>(result[0]);
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
        Assert.Equal(1, result.Length);
        Assert.IsType<IPEndPoint>(result[0]);
    }

    [Fact]
    public void TryCreateEndpoint_InvalidIPAddress_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var invalidIPAddress = "999.999.999.999";

        // Act
        var result = Format.TryCreateEndpoint(invalidIPAddress, 8080, false, loggerMock.Object);

        // Assert
        loggerMock.Verify(
            logger => logger.LogError(
                "No IP address found for hostname:{hostname}",
                It.IsAny<object[]>()
            ),
            Times.Once
        );
        Assert.Null(result);
    }
}
