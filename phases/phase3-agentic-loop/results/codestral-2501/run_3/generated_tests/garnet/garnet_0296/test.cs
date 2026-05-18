using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Threading.Tasks;
using Garnet.common;

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
            x => x.LogError(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Provided hostname does not much acquired machine name")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
        Assert.Null(result);
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
            x => x.LogError(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No IP address found for hostname")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
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
            x => x.LogError(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No reachable IP address found for hostname")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
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
            x => x.LogError(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error while trying to resolve hostname")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
        Assert.Null(result);
    }
}
