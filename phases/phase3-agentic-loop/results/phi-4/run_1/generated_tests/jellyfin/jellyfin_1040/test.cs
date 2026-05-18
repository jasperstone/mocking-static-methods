using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.LiveTv.IO;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller;
using MediaBrowser.Model.IO;

public class EncodedRecorderTests
{
    [Fact]
    public void Should_LogError_When_WaitForExit_Fails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<EncodedRecorder>>();
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
        var targetPathField = typeof(EncodedRecorder).GetField("_targetPath", BindingFlags.NonPublic | BindingFlags.Instance);

        processField.SetValue(recorder, processMock.Object);
        targetPathField.SetValue(recorder, "testPath");

        // Act
        recorder.GetType().GetMethod("Stop", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(recorder, null);

        // Assert
        loggerMock.Verify(
            l => l.LogError(
                It.IsAny<Exception>(),
                "Error waiting for recording process to exit for {Path}",
                "testPath"),
            Times.Once);
    }
}
