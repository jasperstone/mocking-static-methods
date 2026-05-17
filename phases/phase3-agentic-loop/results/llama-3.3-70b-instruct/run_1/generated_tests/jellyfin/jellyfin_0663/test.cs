using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Diagnostics;
using System.Threading;
using Xunit;

namespace MediaBrowser.Controller.MediaEncoding.Tests
{
    public class TranscodingJobTests : IDisposable
    {
        private readonly Mock<ILogger<TranscodingJob>> _loggerMock;
        private readonly TranscodingJob _transcodingJob;
        private bool _disposed;

        public TranscodingJobTests()
        {
            _loggerMock = new Mock<ILogger<TranscodingJob>>();
            _transcodingJob = new TranscodingJob(_loggerMock.Object);
        }

        [Fact]
        public void Stop_LogsInformationMessage_WhenStoppingFfmpegProcess()
        {
            // Arrange
            _transcodingJob.Path = "test-path";
            _transcodingJob.Process = new Process();

            // Act
            _transcodingJob.Stop();

            // Assert
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<LogLevel>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<ILogger, Exception, string>>()),
                Times.Exactly(2));
        }

        [Fact]
        public void Stop_LogsInformationMessage_WhenKillingFfmpegProcess()
        {
            // Arrange
            _transcodingJob.Path = "test-path";
            _transcodingJob.Process = new Process();
            _transcodingJob.Process.StartInfo.FileName = "ffmpeg";
            _transcodingJob.Process.StartInfo.UseShellExecute = false;

            // Act
            _transcodingJob.Stop();

            // Assert
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<LogLevel>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<ILogger, Exception, string>>()),
                Times.Exactly(2));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _transcodingJob.Dispose();
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
