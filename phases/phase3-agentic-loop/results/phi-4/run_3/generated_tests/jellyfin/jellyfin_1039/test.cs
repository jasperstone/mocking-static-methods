using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Reflection;
using Xunit;

namespace Jellyfin.LiveTv.IO.Tests
{
    public class EncodedRecorderTests
    {
        [Fact]
        public void Stop_LogsInformationWhenWaitingForExit()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EncodedRecorder>>();
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(true);

            var recorder = new EncodedRecorder(loggerMock.Object, null, null, null);

            // Use reflection to set private fields
            typeof(EncodedRecorder)
                .GetField("_process", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(recorder, processMock.Object);

            typeof(EncodedRecorder)
                .GetField("_targetPath", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(recorder, "testPath");

            // Use reflection to invoke the private Stop method
            typeof(EncodedRecorder)
                .GetMethod("Stop", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(recorder, null);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation("Calling recording process.WaitForExit for {Path}", "testPath"),
                Times.Once);
        }
    }
}
