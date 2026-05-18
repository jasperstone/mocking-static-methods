using System;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IIS.Core.Tests
{
    public class IISHttpServerTests
    {
        [Fact]
        public void HandleRequest_LogsError_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IISHttpServer>>();
            var server = new IISHttpServerForTest(loggerMock.Object);
            var gch = GCHandle.Alloc(server);
            var ptr = GCHandle.ToIntPtr(gch);

            // Act
            var result = IISHttpServerForTest.InvokeHandleRequest(IntPtr.Zero, ptr);

            // Assert
            Assert.Equal(NativeMethods.REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_FINISH_REQUEST, result);
            loggerMock.Verify(
                x => x.LogError(
                    0,
                    It.IsAny<Exception>(),
                    It.Is<string>(s => s.Contains("Unexpected exception in static IISHttpServer.HandleRequest."))),
                Times.Once);

            gch.Free();
        }

        // Helper subclass to allow injecting logger and bypassing other dependencies
        private class IISHttpServerForTest : IISHttpServer
        {
            public IISHttpServerForTest(ILogger<IISHttpServer> logger)
                : base(
                    new IISNativeApplicationForTest(),
                    new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>().Object,
                    new Mock<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>().Object,
                    new Mock<Microsoft.Extensions.Configuration.IConfiguration>().Object,
                    new Mock<IMemoryPoolFactory<byte>>().Object,
                    new Microsoft.Extensions.Options.OptionsWrapper<IISServerOptions>(new IISServerOptions()),
                    logger)
            {
            }

            public static NativeMethods.REQUEST_NOTIFICATION_STATUS InvokeHandleRequest(IntPtr pInProcessHandler, IntPtr pvRequestContext)
            {
                return HandleRequest(pInProcessHandler, pvRequestContext);
            }
        }

        // Minimal stub for IISNativeApplication to satisfy constructor
        private class IISNativeApplicationForTest : IISNativeApplication
        {
            public override void RegisterCallbacks(
                delegate* unmanaged<IntPtr, IntPtr, NativeMethods.REQUEST_NOTIFICATION_STATUS> pfnRequestHandler,
                delegate* unmanaged<IntPtr, int> pfnShutdownHandler,
                delegate* unmanaged<IntPtr, void> pfnDisconnectHandler,
                delegate* unmanaged<IntPtr, IntPtr, void> pfnAsyncCompletionHandler,
                delegate* unmanaged<IntPtr, void> pfnRequestsDrainedHandler,
                IntPtr pvRequestContext,
                IntPtr pvShutdownContext)
            {
                // no-op
            }

            public override void StopIncomingRequests()
            {
                // no-op
            }

            public override void Stop()
            {
                // no-op
            }
        }

        // Minimal stub for NativeMethods to access REQUEST_NOTIFICATION_STATUS enum
        private static class NativeMethods
        {
            public enum REQUEST_NOTIFICATION_STATUS
            {
                RQ_NOTIFICATION_PENDING = 0,
                RQ_NOTIFICATION_FINISH_REQUEST = 1
            }
        }
    }
}
