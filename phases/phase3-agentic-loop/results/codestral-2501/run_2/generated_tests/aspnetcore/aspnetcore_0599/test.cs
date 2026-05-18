using System;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

public class IISHttpServerTests
{
    [Fact]
    public void HandleRequest_WhenServerIsNull_ReturnsFinishRequest()
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

        var pvRequestContext = GCHandle.ToIntPtr(GCHandle.Alloc(server));

        // Act
        var result = IISHttpServer.HandleRequest(IntPtr.Zero, pvRequestContext);

        // Assert
        Assert.Equal(NativeMethods.REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_FINISH_REQUEST, result);
        loggerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void HandleRequest_WhenExceptionThrown_LogsErrorAndReturnsFinishRequest()
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

        var pvRequestContext = GCHandle.ToIntPtr(GCHandle.Alloc(server));

        // Act
        var result = IISHttpServer.HandleRequest(IntPtr.Zero, pvRequestContext);

        // Assert
        Assert.Equal(NativeMethods.REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_FINISH_REQUEST, result);
        loggerMock.Verify(
            x => x.LogError(
                0,
                It.IsAny<Exception>(),
                It.Is<string>(s => s.Contains("Unexpected exception in static IISHttpServer.HandleRequest."))
            ),
            Times.Once
        );
    }

    [Fact]
    public void HandleShutdown_WhenServerIsNull_ReturnsOne()
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

        var pvRequestContext = GCHandle.ToIntPtr(GCHandle.Alloc(server));

        // Act
        var result = IISHttpServer.HandleShutdown(pvRequestContext);

        // Assert
        Assert.Equal(1, result);
        loggerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void HandleShutdown_WhenExceptionThrown_LogsErrorAndReturnsOne()
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

        var pvRequestContext = GCHandle.ToIntPtr(GCHandle.Alloc(server));

        // Act
        var result = IISHttpServer.HandleShutdown(pvRequestContext);

        // Assert
        Assert.Equal(1, result);
        loggerMock.Verify(
            x => x.LogError(
                0,
                It.IsAny<Exception>(),
                It.Is<string>(s => s.Contains("Unexpected exception in IISHttpServer.HandleShutdown."))
            ),
            Times.Once
        );
    }
}
