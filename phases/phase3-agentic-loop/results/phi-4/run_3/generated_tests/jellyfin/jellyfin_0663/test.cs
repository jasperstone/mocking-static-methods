using System;
using System.Diagnostics;
using System.Threading;
using MediaBrowser.Controller.MediaEncoding;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Tests.MediaEncoding
{
    public class TranscodingJobTests
    {
        [Fact]
        public void Stop_ShouldLogInformationWhenKillingProcess()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var transcodingJob = new TranscodingJob(loggerMock.Object)
            {
                Path = "testPath",
                Process = new Process
                {
                    StandardInput = { CanWrite = true },
                    HasExited = false
                }
            };

            // Act
            transcodingJob.Stop();

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("Killing FFmpeg process for {Path}")),
                    It.Is<object>(o => o.ToString() == "testPath")),
                Times.Once);
        }
    }
}
