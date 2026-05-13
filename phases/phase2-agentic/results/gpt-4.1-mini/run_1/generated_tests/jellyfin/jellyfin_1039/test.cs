using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.LiveTv.IO;

namespace Jellyfin.LiveTv.Tests.IO
{
    public class EncodedRecorderTests
    {
        private class TestEncodedRecorder : EncodedRecorder
        {
            public TestEncodedRecorder(ILogger logger) 
                : base(logger, null, null, null)
            {
            }

            public void SetProcess(Process process)
            {
                _process = process;
            }

            public void SetTargetPath(string path)
            {
                _targetPath = path;
            }

            public void SetHasExited(bool hasExited)
            {
                _hasExited = hasExited;
            }

            public void CallStop()
            {
                Stop();
            }
        }

        [Fact]
        public void Stop_LogsInformationOnWaitForExitCall()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var processMock = new Mock<Process>();
            var standardInputMock = new Mock<TextWriter>();

            processMock.Setup(p => p.StandardInput).Returns(standardInputMock.Object);
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(true);

            var recorder = new TestEncodedRecorder(loggerMock.Object);
            recorder.SetProcess(processMock.Object);
            recorder.SetTargetPath("testpath.ts");
            recorder.SetHasExited(false);

            // Act
            recorder.CallStop();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Calling recording process.WaitForExit for testpath.ts")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            standardInputMock.Verify(s => s.WriteLine("q"), Times.Once);
            processMock.Verify(p => p.WaitForExit(10000), Times.Once);
        }
    }
}
