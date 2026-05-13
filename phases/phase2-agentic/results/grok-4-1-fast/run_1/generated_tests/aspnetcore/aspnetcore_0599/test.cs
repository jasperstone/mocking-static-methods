using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IIS.Core;

public class IISHttpServerTests
{
    [Fact]
    public void HandleRequest_ThrowsException_LogsErrorWithCorrectMessage()
    {
        // Arrange
        var logger = new Mock<ILogger<IISHttpServer>>();
        var applicationLifetime = new Mock<IHostApplicationLifetime>();
        var nativeApplication = new Mock<IISNativeApplication>();
        var memoryPoolFactory = Mock.Of<IMemoryPoolFactory<byte>>();
        var options = new IISServerOptions();

        var server = new IISHttpServer(
            nativeApplication.Object,
            applicationLifetime.Object,
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IConfiguration>(),
            memoryPoolFactory,
            Options.Create(options),
            logger.Object);

        server.StartAsync(Mock.Of<IHttpApplication< object>>(), default);

        var invalidGCHandle = GCHandle.ToIntPtr(GCHandle.Alloc(null)); // Invalid GC handle to force exception

        // Act
        var result = IISHttpServer.HandleRequest(IntPtr.Zero, invalidGCHandle);

        // Assert
        logger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat<string>>((v, t) => v.ToString().Contains("Unexpected exception in static IISHttpServer.HandleRequest.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
            Times.Once);

        Assert.Equal(NativeMethods.REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_FINISH_REQUEST, result);
    }

    [Fact]
    public void HandleShutdown_ThrowsException_LogsErrorWithCorrectMessage()
    {
        // Arrange
        var logger = new Mock<ILogger<IISHttpServer>>();
        var applicationLifetime = new Mock<IHostApplicationLifetime>();
        var nativeApplication = new Mock<IISNativeApplication>();
        var memoryPoolFactory = Mock.Of<IMemoryPoolFactory<byte>>();
        var options = new IISServerOptions();

        var server = new IISHttpServer(
            nativeApplication.Object,
            applicationLifetime.Object,
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IConfiguration>(),
            memoryPoolFactory,
            Options.Create(options),
            logger.Object);

        var validGCHandle = GCHandle.ToIntPtr(GCHandle.Alloc(server));
        applicationLifetime.Setup(al => al.StopApplication()).Throws(new InvalidOperationException("Test exception"));

        // Act
        var result = IISHttpServer.HandleShutdown(validGCHandle);

        // Assert
        logger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat<string>>((v, t) => v.ToString().Contains("Unexpected exception in IISHttpServer.HandleShutdown.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
            Times.Once);

        Assert.Equal(1, result);
    }
}
