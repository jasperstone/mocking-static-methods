using System;
using System.Diagnostics;
using System.Reflection;
using Jellyfin.LiveTv.IO;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.MediaEncoding;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.LiveTv.Tests.IO
{
    public class EncodedRecorderStopTests
    {
        [Fact]
        public void Stop_LogsInformationBeforeWaitingForExit()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mediaEncoder = Mock.Of<IMediaEncoder>();
            var appPaths = Mock.Of<IServerApplicationPaths>();
            var serverConfigurationManager = Mock.Of<IServerConfigurationManager>();

            var recorder = new EncodedRecorder(loggerMock.Object, mediaEncoder, appPaths, serverConfigurationManager);
            const string targetPath = "test.ts";

            var process = new Process
            {
                StartInfo =
                {
                    RedirectStandardInput = true
                }
            };

            SetPrivateField(recorder, "_targetPath", targetPath);
            SetPrivateField(recorder, "_process", process);

            try
            {
                // Act
                InvokeStop(recorder);
            }
            finally
            {
                process.Dispose();
            }

            // Assert
            var expectedMessage = $"Calling recording process.WaitForExit for {targetPath}";

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => string.Equals(v.ToString(), expectedMessage, StringComparison.Ordinal)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private static void SetPrivateField<T>(object instance, string fieldName, T value)
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            field.SetValue(instance, value);
        }

        private static void InvokeStop(EncodedRecorder recorder)
        {
            var method = typeof(EncodedRecorder).GetMethod("Stop", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            method.Invoke(recorder, null);
        }
    }
}
