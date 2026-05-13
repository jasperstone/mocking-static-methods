using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.LiveTv.IO.Tests
{
    public class EncodedRecorderTests
    {
        [Fact]
        public void Should_LogError_When_WaitForExit_Throws_Exception()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var processMock = new Mock<Process>();
            var recorder = new EncodedRecorder(loggerMock.Object, null, null, null)
            {
                _process = processMock.Object,
                _targetPath = "testPath"
            };

            // Simulate an exception during WaitForExit
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Throws(new InvalidOperationException("Test exception"));

            // Act
            recorder.Stop();

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.Is<Exception>(e => e.Message == "Test exception"),
                    "Error waiting for recording process to exit for {Path}",
                    It.Is<object[]>(args => args[0].ToString() == "testPath")),
                Times.Once);
        }
    }
}
