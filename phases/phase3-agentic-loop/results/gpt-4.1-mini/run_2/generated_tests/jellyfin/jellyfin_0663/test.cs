using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.MediaEncoding;

namespace MediaBrowser.Controller.MediaEncoding.Tests
{
    public class TranscodingJobTests
    {
        private class FakeProcess : IDisposable
        {
            public bool KillCalled { get; private set; }
            public bool WaitForExitReturnValue { get; set; } = false;
            public StringWriter StandardInputWriter { get; } = new();

            public void WriteLineToStandardInput(string line)
            {
                StandardInputWriter.WriteLine(line);
            }

            public bool WaitForExit(int milliseconds)
            {
                return WaitForExitReturnValue;
            }

            public void Kill()
            {
                KillCalled = true;
            }

            public void Dispose()
            {
                StandardInputWriter.Dispose();
            }
        }

        [Fact]
        public void Stop_LogsInformationAndKillsProcess_WhenProcessDoesNotExit()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var transcodingJob = new TranscodingJob(loggerMock.Object)
            {
                Path = "testpath",
                HasExited = false
            };

            var fakeProcess = new FakeProcess();

            // We cannot set Process property directly because it's not virtual and no setter,
            // so we use reflection to set the private backing field.
            var processField = typeof(TranscodingJob).GetProperty("Process");
            var processBackingField = typeof(TranscodingJob).GetField("<Process>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (processBackingField != null)
            {
                processBackingField.SetValue(transcodingJob, fakeProcess);
            }
            else
            {
                // fallback: use reflection to set private field _process if exists
                var privateField = typeof(TranscodingJob).GetField("_process", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (privateField != null)
                {
                    privateField.SetValue(transcodingJob, fakeProcess);
                }
                else
                {
                    throw new InvalidOperationException("Cannot set Process property or backing field");
                }
            }

            // Act
            // We need to call Stop method, but it uses Process.StandardInput.WriteLine and WaitForExit and Kill.
            // So we simulate those by replacing Process with our fakeProcess and using reflection to call Stop.

            // Call Stop method
            transcodingJob.Stop();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping ffmpeg process with q command for")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Killing FFmpeg process for")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.True(fakeProcess.KillCalled);
            Assert.Contains("q", fakeProcess.StandardInputWriter.ToString());
        }

        [Fact]
        public void Stop_DoesNotKillProcess_WhenProcessExitsWithinTimeout()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var transcodingJob = new TranscodingJob(loggerMock.Object)
            {
                Path = "testpath",
                HasExited = false
            };

            var fakeProcess = new FakeProcess { WaitForExitReturnValue = true };

            var processBackingField = typeof(TranscodingJob).GetField("<Process>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (processBackingField != null)
            {
                processBackingField.SetValue(transcodingJob, fakeProcess);
            }
            else
            {
                var privateField = typeof(TranscodingJob).GetField("_process", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (privateField != null)
                {
                    privateField.SetValue(transcodingJob, fakeProcess);
                }
                else
                {
                    throw new InvalidOperationException("Cannot set Process property or backing field");
                }
            }

            // Act
            transcodingJob.Stop();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping ffmpeg process with q command for")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Killing FFmpeg process for")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);

            Assert.False(fakeProcess.KillCalled);
            Assert.Contains("q", fakeProcess.StandardInputWriter.ToString());
        }

        [Fact]
        public void Stop_DoesNothing_WhenHasExitedIsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var transcodingJob = new TranscodingJob(loggerMock.Object)
            {
                HasExited = true
            };

            var fakeProcess = new FakeProcess();

            var processBackingField = typeof(TranscodingJob).GetField("<Process>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (processBackingField != null)
            {
                processBackingField.SetValue(transcodingJob, fakeProcess);
            }
            else
            {
                var privateField = typeof(TranscodingJob).GetField("_process", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (privateField != null)
                {
                    privateField.SetValue(transcodingJob, fakeProcess);
                }
                else
                {
                    throw new InvalidOperationException("Cannot set Process property or backing field");
                }
            }

            // Act
            transcodingJob.Stop();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);

            Assert.False(fakeProcess.KillCalled);
            Assert.Empty(fakeProcess.StandardInputWriter.ToString());
        }
    }
}
