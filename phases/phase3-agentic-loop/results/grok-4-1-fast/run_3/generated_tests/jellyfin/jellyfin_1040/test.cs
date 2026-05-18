using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.LiveTv.IO;

namespace Jellyfin.LiveTv.Tests.IO
{
    public class EncodedRecorderTests
    {
        private readonly Mock<ILogger<EncodedRecorder>> _loggerMock;
        private readonly EncodedRecorder _recorder;

        public EncodedRecorderTests()
        {
            _loggerMock = new Mock<ILogger<EncodedRecorder>>();
            _recorder = new EncodedRecorder(_loggerMock.Object, null!, null!, null!);
        }

        [Fact]
        public void Stop_WaitForExit_ThrowsException_LogsError()
        {
            // Arrange
            var targetPath = "/test/path/recording.ts";
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.WaitForExit(10000)).Throws(new InvalidOperationException("Process error"));
            
            // Use reflection to set private fields
            SetPrivateField("_targetPath", targetPath);
            SetPrivateField("_process", processMock.Object);
            SetPrivateField("_hasExited", false);

            // Act
            _recorder.Stop();

            // Assert - verifies LogError call on line 240
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ((IReadOnlyList<KeyValuePair<string, object?>>?)v)?.ToString()?.Contains("Error waiting for recording process to exit for " + targetPath) == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private void SetPrivateField(string fieldName, object value)
        {
            typeof(EncodedRecorder).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(_recorder, value);
        }
    }
}
