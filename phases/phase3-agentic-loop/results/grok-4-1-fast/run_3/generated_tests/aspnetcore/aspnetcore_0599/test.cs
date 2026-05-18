using System;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IIS.Core.Tests;

public class IISHttpServerTests
{
    [Fact]
    public void HandleRequest_ExceptionPath_LogsErrorMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<IISHttpServer>>();
        mockLogger.Setup(l => l.Log(
            It.Is<LogLevel>(level => level == LogLevel.Error),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => ((string)v).ToString().Contains("Unexpected exception in static IISHttpServer.HandleRequest.")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        var applicationLifetime = new Mock<IHostApplicationLifetime>();
        var nativeApplication = new Mock<IISNativeApplication>();
        var authentication = new Mock<IAuthenticationSchemeProvider>();
        var configuration = new Mock<IConfiguration>();
        var memoryPoolFactory = new Mock<IMemoryPoolFactory<byte>>();
        var options = new IISServerOptions();

        var server = new IISHttpServer(
            nativeApplication.Object,
            applicationLifetime.Object,
            authentication.Object,
            configuration.Object,
            memoryPoolFactory.Object,
            Options.Create(options),
            mockLogger.Object);

        // Use reflection to get _httpServerHandle after StartAsync
        server.StartAsync(Mock.Of<IHttpApplication<HttpContext>>(), default).Wait();

        var handleField = typeof(IISHttpServer)
            .GetField("_httpServerHandle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var gcHandle = (GCHandle)handleField!.GetValue(server)!;
        var pvRequestContext = GCHandle.ToIntPtr(gcHandle);
        var pInProcessHandler = Marshal.AllocHGlobal(1);

        try
        {
            // Temporarily set Target to null to force exception path after null check
            var originalTarget = gcHandle.Target;
            gcHandle.Target = null;

            // Act - this will hit the catch block
            _ = IISHttpServer.HandleRequest(pInProcessHandler, pvRequestContext);
        }
        finally
        {
            Marshal.FreeHGlobal(pInProcessHandler);
            // Restore
            gcHandle.Target = server;
        }

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.Is<EventId>(e => e.Id == 0),
                It.Is<It.IsAnyType>((v, t) => ((string)v).ToString().Contains("Unexpected exception in static IISHttpServer.HandleRequest.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void HandleShutdown_ExceptionPath_LogsErrorMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<IISHttpServer>>();
        mockLogger.Setup(l => l.Log(
            It.Is<LogLevel>(level => level == LogLevel.Error),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => ((string)v).ToString().Contains("Unexpected exception in IISHttpServer.HandleShutdown.")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        var applicationLifetime = new Mock<IHostApplicationLifetime>();
        applicationLifetime.Setup(al => al.StopApplication()).Throws(new InvalidOperationException("Test"));
        
        var nativeApplication = new Mock<IISNativeApplication>();
        var authentication = new Mock<IAuthenticationSchemeProvider>();
        var configuration = new Mock<IConfiguration>();
        var memoryPoolFactory = new Mock<IMemoryPoolFactory<byte>>();
        var options = new IISServerOptions();

        var server = new IISHttpServer(
            nativeApplication.Object,
            applicationLifetime.Object,
            authentication.Object,
            configuration.Object,
            memoryPoolFactory.Object,
            Options.Create(options),
            mockLogger.Object);

        server.StartAsync(Mock.Of<IHttpApplication<HttpContext>>(), default).Wait();

        var handleField = typeof(IISHttpServer)
            .GetField("_httpServerHandle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var gcHandle = (GCHandle)handleField!.GetValue(server)!;
        var pvRequestContext = GCHandle.ToIntPtr(gcHandle);

        // Act
        var result = IISHttpServer.HandleShutdown(pvRequestContext);

        // Assert
        Assert.Equal(1, result);
        
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.Is<EventId>(e => e.Id == 0),
                It.Is<It.IsAnyType>((v, t) => ((string)v).ToString().Contains("Unexpected exception in IISHttpServer.HandleShutdown.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
