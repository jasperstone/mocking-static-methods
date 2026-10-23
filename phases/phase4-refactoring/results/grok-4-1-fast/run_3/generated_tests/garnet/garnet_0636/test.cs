using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Tsavorite.core;
using Xunit;

public class DeltaLogTests
{
    private readonly Mock<IDevice> _mockDevice;
    private readonly Mock<ILogger> _mockLogger;
    private readonly DeltaLog _deltaLog;
    private readonly PageAsyncFlushResult<Empty> _result;
    private readonly MethodInfo _callbackMethod;

    public DeltaLogTests()
    {
        _mockDevice = new Mock<IDevice>();
        _mockLogger = new Mock<ILogger>();

        _mockDevice.Setup(d => d.SectorSize).Returns(512u);
        _mockDevice.Setup(d => d.GetFileSize(0)).Returns(0L);

        _deltaLog = new DeltaLog(_mockDevice.Object, 12, 0, _mockLogger.Object);
        _result = new PageAsyncFlushResult<Empty> { count = 1 };

        _callbackMethod = typeof(DeltaLog).GetMethod("AsyncFlushPageToDeviceCallback", 
            BindingFlags.NonPublic | BindingFlags.Instance)!;
    }

    [Fact]
    public void AsyncFlushPageToDeviceCallback_LogsError_WhenErrorCodeIsNonZero()
    {
        // Arrange
        uint errorCode = 1001;
        uint numBytes = 4096;

        // Act
        _callbackMethod.Invoke(_deltaLog, new object[] { errorCode, numBytes, _result });

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AsyncFlushPageToDeviceCallback error") && v.ToString().Contains("1001")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void AsyncFlushPageToDeviceCallback_DoesNotLogError_WhenErrorCodeIsZero()
    {
        // Arrange
        uint errorCode = 0;
        uint numBytes = 4096;

        // Act
        _callbackMethod.Invoke(_deltaLog, new object[] { errorCode, numBytes, _result });

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
