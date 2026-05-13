using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Runtime.InteropServices;
using Xunit;

namespace Tests
{
    public class IISHttpServerTests
    {
        [Fact]
        public void HandleRequest_LogsError_OnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IISHttpServer>>();
            var server = new IISHttpServer(
                new IISNativeApplication(),
                new HostApplicationLifetime(),
                new AuthenticationSchemeProvider(),
                new ConfigurationBuilder().Build(),
                new MemoryPoolFactory<byte>(),
                Options.Create(new IISServerOptions()),
                loggerMock.Object
            );

            var exception = new Exception("Test exception");
            var pvRequestContext = GCHandle.Alloc(server).AddrOfPinnedObject();

            // Act and Assert
            try
            {
                IISHttpServer.HandleRequest(IntPtr.Zero, pvRequestContext);
                throw new InvalidOperationException("Expected exception to be thrown");
            }
            catch (Exception ex)
            {
                loggerMock.Verify(
                    l => l.LogError(
                        0,
                        ex,
                        $"Unexpected exception in static {nameof(IISHttpServer)}.{nameof(IISHttpServer.HandleRequest)}."),
                    Times.Once);
            }
            finally
            {
                GCHandle.FromIntPtr(pvRequestContext).Free();
            }
        }

        [Fact]
        public void HandleShutdown_LogsError_OnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IISHttpServer>>();
            var server = new IISHttpServer(
                new IISNativeApplication(),
                new HostApplicationLifetime(),
                new AuthenticationSchemeProvider(),
                new ConfigurationBuilder().Build(),
                new MemoryPoolFactory<byte>(),
                Options.Create(new IISServerOptions()),
                loggerMock.Object
            );

            var exception = new Exception("Test exception");
            var pvRequestContext = GCHandle.Alloc(server).AddrOfPinnedObject();

            // Act and Assert
            try
            {
                IISHttpServer.HandleShutdown(pvRequestContext);
                throw new InvalidOperationException("Expected exception to be thrown");
            }
            catch (Exception ex)
            {
                loggerMock.Verify(
                    l => l.LogError(
                        0,
                        ex,
                        $"Unexpected exception in {nameof(IISHttpServer)}.{nameof(IISHttpServer.HandleShutdown)}."),
                    Times.Once);
            }
            finally
            {
                GCHandle.FromIntPtr(pvRequestContext).Free();
            }
        }
    }
}
