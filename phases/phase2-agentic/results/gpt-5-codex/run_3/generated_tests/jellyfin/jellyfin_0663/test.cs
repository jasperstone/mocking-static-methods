using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Tests.MediaEncoding
{
    public class TranscodingJobTests
    {
        [Fact]
        public void Stop_WhenProcessDoesNotExitWithinTimeout_LogsKillInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var expectedPath = "/tmp/test-path";

            var job = new TranscodingJob(loggerMock.Object)
            {
                Path = expectedPath
            };

            using var process = StartLongRunningProcess();
            job.Process = process;

            try
            {
                // Act
                job.Stop();

                // Ensure the process has been terminated by Stop().
                Assert.True(process.WaitForExit(5000), "The process should be terminated by the Stop method.");
            }
            finally
            {
                if (!process.HasExited)
                {
                    process.Kill();
                    process.WaitForExit();
                }

                job.Dispose();
            }

            // Assert
            var expectedMessage = $"Killing FFmpeg process for {expectedPath}";
            var killingLog = loggerMock.Invocations.SingleOrDefault(invocation =>
                invocation.Method.Name == nameof(ILogger.Log)
                && invocation.Arguments.Count == 5
                && invocation.Arguments[0] is LogLevel level && level == LogLevel.Information
                && invocation.Arguments[2]?.ToString() == expectedMessage);

            Assert.NotNull(killingLog);

            if (killingLog is not null)
            {
                var state = killingLog.Arguments[2];
                var keyValuePairs = state as IEnumerable<KeyValuePair<string, object>>;
                Assert.NotNull(keyValuePairs);

                var pathPair = keyValuePairs!.FirstOrDefault(kvp => string.Equals(kvp.Key, "Path", StringComparison.Ordinal));
                Assert.Equal(expectedPath, pathPair.Value);
            }
        }

        private static Process StartLongRunningProcess()
        {
            ProcessStartInfo startInfo;

            if (OperatingSystem.IsWindows())
            {
                startInfo = new ProcessStartInfo("ping", "127.0.0.1 -n 10")
                {
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
            }
            else
            {
                startInfo = new ProcessStartInfo("sleep", "10")
                {
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };
            }

            var process = Process.Start(startInfo);
            Assert.NotNull(process);

            return process!;
        }
    }
}
