using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.LiveTv.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class EncodedRecorderTests
{
    private readonly Mock<ILogger<EncodedRecorder>> _loggerMock;
    private readonly Mock<IMediaEncoder> _mediaEncoderMock;
    private readonly Mock<IServerApplicationPaths> _appPathsMock;
    private readonly Mock<IServerConfigurationManager> _serverConfigMock;
    private readonly EncodedRecorder _recorder;

    public EncodedRecorderTests()
    {
        _loggerMock = new Mock<ILogger<EncodedRecorder>>();
        _mediaEncoderMock = new Mock<IMediaEncoder>();
        _appPathsMock = new Mock<IServerApplicationPaths>();
        _serverConfigMock = new Mock<IServerConfigurationManager>();
        _recorder = new EncodedRecorder(_loggerMock.Object, _mediaEncoderMock.Object, _appPathsMock.Object, _serverConfigMock.Object);
    }

    [Fact]
    public void Stop_WaitForExit_ThrowsException_LogsError()
    {
        // Arrange
        var targetPath = "/test/path/recording.ts";
        var processMock = new Mock<Process>();
        processMock.Setup(p => p.WaitForExit(10000)).Throws(new InvalidOperationException("Process error"));
        _recorder.GetType().GetField("_targetPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(_recorder, targetPath);
        _recorder.GetType().GetField("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(_recorder, processMock.Object);
        _recorder.GetType().GetField("_hasExited", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(_recorder, false);

        // Act
        _recorder.Stop();

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat<string>>(s => s.ToString().Contains("Error waiting for recording process to exit for " + targetPath)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Stop_Kill_ThrowsException_LogsError()
    {
        // Arrange
        var targetPath = "/test/path/recording.ts";
        var processMock = new Mock<Process>();
        processMock.Setup(p => p.WaitForExit(10000)).Returns(true);
        processMock.Setup(p => p.Kill()).Throws(new InvalidOperationException("Kill error"));
        _recorder.GetType().GetField("_targetPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(_recorder, targetPath);
        _recorder.GetType().GetField("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(_recorder, processMock.Object);
        _recorder.GetType().GetField("_hasExited", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(_recorder, false);

        // Act
        _recorder.Stop();

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat<string>>(s => s.ToString().Contains("Error killing recording transcoding job for " + targetPath)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Stop_StandardInputWrite_ThrowsException_LogsError()
    {
        // Arrange
        var targetPath = "/test/path/recording.ts";
        var processMock = new Mock<Process>();
        var streamWriterMock = new Mock<StreamWriter>();
        streamWriterMock.Setup(x => x.WriteLine("q")).Throws(new IOException("Write error"));
        processMock.Setup(p => p.StandardInput).Returns(streamWriterMock.Object);
        _recorder.GetType().GetField("_targetPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(_recorder, targetPath);
        _recorder.GetType().GetField("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(_recorder, processMock.Object);
        _recorder.GetType().GetField("_hasExited", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(_recorder, false);

        // Act
        _recorder.Stop();

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat<string>>(s => s.ToString().Contains("Error stopping recording transcoding job for " + targetPath)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
            Times.Once);
    }
}
