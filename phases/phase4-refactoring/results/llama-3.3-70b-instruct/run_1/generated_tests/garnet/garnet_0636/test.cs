using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Tsavorite.core;

public class DeltaLogTests
{
    [Fact]
    public void AsyncFlushPageToDeviceCallback_LogsErrorOnFailure()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var deviceMock = new Mock<IDevice>();
        deviceMock.Setup(d => d.SectorSize).Returns(512);
        deviceMock.Setup(d => d.GetFileSize(0)).Returns(1024);
        var deltaLog = new DeltaLog(deviceMock.Object, 10, 0, loggerMock.Object);

        // Act
        deltaLog.AsyncFlushPageToDeviceCallback(1, 0, null);

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
    }
}
