using System;
using System.Diagnostics;
using System.IO;
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
                    StartInfo = new ProcessStartInfo
                    {
                        UseShellExecute = false,
                        RedirectStandardInput = true
                    }
                }
            };
            transcodingJob.Process.Start();

            // Act
            transcodingJob.Stop();

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s == "Killing FFmpeg process for {Path}"),
                    It.Is<object[]>(o => o[0].ToString() == "testPath")),
                Times.Once);
        }
    }
}
