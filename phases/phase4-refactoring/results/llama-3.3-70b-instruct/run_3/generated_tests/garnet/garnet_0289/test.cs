using Xunit;
using Moq;
using System;
using System.Net;
using Microsoft.Extensions.Logging;
using Garnet.common;

public class FormatTests
{
    [Fact]
    public void TryCreateEndpoint_NoIpAddresses_ReturnsNull()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var singleAddressOrHostname = "example.com";

        // Act
        var result = Format.TryCreateEndpoint(singleAddressOrHostname, 8080, logger: loggerMock.Object);

        // Assert
        Assert.Null(result);
        loggerMock.Verify(l => l.LogError(It.IsAny<string>(), singleAddressOrHostname), Times.Once);
    }

    [Fact]
    public void TryCreateEndpoint_NoReachableIpAddresses_ReturnsNull()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var singleAddressOrHostname = "example.com";

        // Act
        var result = Format.TryCreateEndpoint(singleAddressOrHostname, 8080, tryConnect: true, logger: loggerMock.Object);

        // Assert
        Assert.Null(result);
        loggerMock.Verify(l => l.LogError(It.IsAny<string>(), singleAddressOrHostname), Times.Once);
    }

    [Fact]
    public void TryCreateEndpoint_InvalidHostname_ReturnsNull()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var singleAddressOrHostname = "invalid-hostname";

        // Act
        var result = Format.TryCreateEndpoint(singleAddressOrHostname, 8080, logger: loggerMock.Object);

        // Assert
        Assert.Null(result);
        loggerMock.Verify(l => l.LogError(It.IsAny<string>(), singleAddressOrHostname), Times.Once);
    }

    [Fact]
    public void TryCreateEndpoint_ValidIpAddress_ReturnsEndPoint()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var singleAddressOrHostname = "127.0.0.1";

        // Act
        var result = Format.TryCreateEndpoint(singleAddressOrHostname, 8080, logger: loggerMock.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.IsType<IPEndPoint>(result[0]);
    }

    [Fact]
    public void TryCreateEndpoint_ValidHostname_ReturnsEndPoint()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var singleAddressOrHostname = "localhost";

        // Act
        var result = Format.TryCreateEndpoint(singleAddressOrHostname, 8080, logger: loggerMock.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.IsType<IPEndPoint>(result[0]);
    }
}
