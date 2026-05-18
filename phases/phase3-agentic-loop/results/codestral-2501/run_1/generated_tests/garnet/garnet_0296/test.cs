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
        var invalidHostname = "invalidHostname";

        // Act
        var result = await Format.TryCreateEndpointAsync(invalidHostname, 8080, false, loggerMock.Object);

        // Assert
        loggerMock.Verify(
            x => x.LogError(
                "Provided hostname does not much acquired machine name {addressOrHostname} {machineHostname}!",
                It.IsAny<object[]>()),
            Times.Once);
        Assert.Null(result);
    }
}
