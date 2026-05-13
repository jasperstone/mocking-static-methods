using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild.Tests
{
    public class BuildHostProcessManagerTests
    {
        [Fact]
        public void LogProcessFailure_ProcessExitedWithNonZeroCode_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dummy.exe",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };
            process.Start();
            process.WaitForExit();
            process.ExitCode = 1; // Simulate a non-zero exit code

            var processManager = new BuildHostProcessManager(loggerFactory: Mock.Of<ILoggerFactory>(factory => factory.CreateLogger<BuildHostProcessManager>() == mockLogger.Object));

            // Act
            processManager.LogProcessFailure();

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
