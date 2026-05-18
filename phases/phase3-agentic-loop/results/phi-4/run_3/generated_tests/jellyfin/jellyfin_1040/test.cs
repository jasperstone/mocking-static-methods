using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Extensions;
using Jellyfin.Extensions.Json;
using Jellyfin.LiveTv.IO;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class EncodedRecorderTests
{
    [Fact]
    public async Task ShouldLogErrorWhenWaitingForProcessExitFails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var mediaEncoderMock = new Mock<IMediaEncoder>();
        var appPathsMock = new Mock<IServerApplicationPaths>();
        var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();

        var recorder = new EncodedRecorder(
            loggerMock.Object,
            mediaEncoderMock.Object,
            appPathsMock.Object,
            serverConfigurationManagerMock.Object);

        var processMock = new Mock<Process>();
        processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(false);

        // Use reflection to set private fields
        var processField = typeof(EncodedRecorder).GetField("_process", BindingFlags.NonPublic | BindingFlags.Instance);
        var hasExitedField = typeof(EncodedRecorder).GetField("_hasExited", BindingFlags.NonPublic | BindingFlags.Instance);

        processField.SetValue(recorder, processMock.Object);
        hasExitedField.SetValue(recorder, false);

        // Use reflection to invoke the private Stop method
        var stopMethod = typeof(EncodedRecorder).GetMethod("Stop", BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        await Task.Run(() => stopMethod.Invoke(recorder, null));

        // Assert
        loggerMock.Verify(
            l => l.LogError(
                It.IsAny<Exception>(),
                "Error waiting for recording process to exit for {Path}",
                It.IsAny<string>()),
            Times.Once);
    }
}
