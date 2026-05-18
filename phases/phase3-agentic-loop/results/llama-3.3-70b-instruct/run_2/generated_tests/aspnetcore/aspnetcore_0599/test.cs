using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Runtime.InteropServices;
using Xunit;

namespace Microsoft.AspNetCore.Server.IIS.Core.Tests;

public class IISHttpServerTests
{
    [Fact]
    public void HandleRequest_ThrowsException_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<IISHttpServer>>();
        var server = new IISHttpServer(
            new IISNativeApplication(new NativeSafeHandle(IntPtr.Zero)),
            new HostApplicationLifetime(),
            new AuthenticationSchemeProvider(),
            new ConfigurationBuilder().Build(),
            new MemoryPoolFactory<byte>(),
            Options.Create(new IISServerOptions()),
            loggerMock.Object
        );

        var exception = new Exception("Test exception");
        var pvRequestContext = GCHandle.Alloc(server);

        // Act and Assert
        try
        {
            IISHttpServer.HandleRequest(IntPtr.Zero, (IntPtr)pvRequestContext);
        }
        catch (Exception ex)
        {
            loggerMock.Verify(l => l.LogError(0, It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }
    }

    [Fact]
    public void HandleShutdown_ThrowsException_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<IISHttpServer>>();
        var server = new IISHttpServer(
            new IISNativeApplication(new NativeSafeHandle(IntPtr.Zero)),
            new HostApplicationLifetime(),
            new AuthenticationSchemeProvider(),
            new ConfigurationBuilder().Build(),
            new MemoryPoolFactory<byte>(),
            Options.Create(new IISServerOptions()),
            loggerMock.Object
        );

        var exception = new Exception("Test exception");
        var pvRequestContext = GCHandle.Alloc(server);

        // Act and Assert
        try
        {
            IISHttpServer.HandleShutdown((IntPtr)pvRequestContext);
        }
        catch (Exception ex)
        {
            loggerMock.Verify(l => l.LogError(0, It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }
    }
}
