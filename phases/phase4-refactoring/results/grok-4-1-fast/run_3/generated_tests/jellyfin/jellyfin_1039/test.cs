using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.LiveTv.IO.Tests
{
    public class EncodedRecorderTests
    {
        private readonly Mock<ILogger<EncodedRecorder>> _loggerMock;
        private readonly EncodedRecorder _recorder;

        public EncodedRecorderTests()
        {
            _loggerMock = new Mock<ILogger<EncodedRecorder>>();
            
            // Create minimal mocks using object to avoid missing type references
            var mediaEncoderMock = new object();
            var appPathsMock = new object();
            var serverConfigMock = new object();
            
            _recorder = new EncodedRecorder(_loggerMock.Object, (global::MediaBrowser.Controller.MediaEncoding.IMediaEncoder)mediaEncoderMock, (global::MediaBrowser.Common.IServerApplicationPaths)appPathsMock, (global::MediaBrowser.Common.Configuration.IServerConfigurationManager)serverConfigMock);
            
            // Set up private field for testing
            typeof(EncodedRecorder).GetField("_targetPath", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(_recorder, "/test/path.mkv");
        }

        [Fact]
        public void Stop_WhenProcessWaitForExitCalled_LogsInformationMessage()
        {
            // Arrange
            var mockProcess = new Mock<Process>();
            mockProcess.Setup(p => p.WaitForExit(10000)).Returns(true);
            
            typeof(EncodedRecorder).GetField("_process", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(_recorder, mockProcess.Object);
            typeof(EncodedRecorder).GetField("_hasExited", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(_recorder, false);

            // Act - invoke private Stop method via reflection
            var stopMethod = typeof(EncodedRecorder).GetMethod("Stop", BindingFlags.NonPublic | BindingFlags.Instance);
            stopMethod?.Invoke(_recorder, null);

            // Assert - verify LogInformation was called with correct message
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Calling recording process.WaitForExit for /test/path.mkv")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
