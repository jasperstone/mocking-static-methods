using System;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class IISHttpServerTests
{
    [Fact]
    public void HandleRequest_LogsError_WhenExceptionIsThrown()
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
            x => x.LogError(
                0,
                It.IsAny<Exception>(),
                It.Is<string>(s => s.Contains("Unexpected exception in static IISHttpServer.HandleRequest."))),
            Times.Once);

        Assert.Equal(NativeMethods.REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_FINISH_REQUEST, result);

        gcHandle.Free();
    }

    [Fact]
    public void HandleShutdown_LogsError_WhenExceptionIsThrown()
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
            x => x.LogError(
                0,
                It.IsAny<Exception>(),
                It.Is<string>(s => s.Contains("Unexpected exception in IISHttpServer.HandleShutdown."))),
            Times.Once);

        Assert.Equal(1, result);

        gcHandle.Free();
    }
}
