using System;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class IISHttpServerTests
{
    [Fact]
    public void HandleRequest_LogsError_WhenExceptionThrown()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<IISHttpServer>>();
        var server = new IISHttpServer(
            Mock.Of<IISNativeApplication>(),
            Mock.Of<IHostApplicationLifetime>(),
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IConfiguration>(),
            Mock.Of<IMemoryPoolFactory<byte>>(),
            Mock.Of<IOptions<IISServerOptions>>(),
            loggerMock.Object
        );

        var gcHandle = GCHandle.Alloc(server);
        var pvRequestContext = GCHandle.ToIntPtr(gcHandle);

        // Act
        var result = IISHttpServer.HandleRequest(IntPtr.Zero, pvRequestContext);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unexpected exception in static IISHttpServer.HandleRequest.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);

        Assert.Equal(NativeMethods.REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_FINISH_REQUEST, result);
    }

    [Fact]
    public void HandleShutdown_LogsError_WhenExceptionThrown()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<IISHttpServer>>();
        var server = new IISHttpServer(
            Mock.Of<IISNativeApplication>(),
            Mock.Of<IHostApplicationLifetime>(),
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IConfiguration>(),
            Mock.Of<IMemoryPoolFactory<byte>>(),
            Mock.Of<IOptions<IISServerOptions>>(),
            loggerMock.Object
        );

        var gcHandle = GCHandle.Alloc(server);
        var pvRequestContext = GCHandle.ToIntPtr(gcHandle);

        // Act
        var result = IISHttpServer.HandleShutdown(pvRequestContext);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unexpected exception in IISHttpServer.HandleShutdown.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);

        Assert.Equal(1, result);
    }
}
