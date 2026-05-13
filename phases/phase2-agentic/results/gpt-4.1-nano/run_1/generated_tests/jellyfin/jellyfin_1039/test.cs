using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.LiveTv.IO.Tests
{
    public class EncodedRecorderTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IMediaEncoder> _mediaEncoderMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;
        private readonly Mock<IServerConfigurationManager> _configManagerMock;

        public EncodedRecorderTests()
        {
            _loggerMock = new Mock<ILogger>();
            _mediaEncoderMock = new Mock<IMediaEncoder>();
            _appPathsMock = new Mock<IServerApplicationPaths>();
            _configManagerMock = new Mock<IServerConfigurationManager>();
        }

        [Fact]
        public async Task Stop_ShouldLogInformation_WhenProcessIsRunning()
        {
            // Arrange
            var recorder = new TestEncodedRecorder(
                _loggerMock.Object,
                _mediaEncoderMock.Object,
                _appPathsMock.Object,
                _configManagerMock.Object);

            var processMock = new Mock<Process>();
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(false);
            processMock.Setup(p => p.Kill());
            processMock.Setup(p => p.StandardInput).Returns(new Mock<StreamWriter>().Object);
            processMock.Setup(p => p.Exited).Returns(true);

            recorder.SetProcess(processMock.Object);
            recorder.SetTargetPath("testPath");
            recorder.SetHasExited(false);

            // Act
            await recorder.InvokeStopAsync();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Calling recording process.WaitForExit for testPath")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Helper subclass to access protected members and set internal state
    public class TestEncodedRecorder : EncodedRecorder
    {
        public TestEncodedRecorder(ILogger logger, IMediaEncoder mediaEncoder, IServerApplicationPaths appPaths, IServerConfigurationManager serverConfigurationManager)
            : base(logger, mediaEncoder, appPaths, serverConfigurationManager)
        {
        }

        private Process _testProcess;
        private string _testTargetPath;
        private bool _testHasExited;

        public void SetProcess(Process process) => _testProcess = process;
        public void SetTargetPath(string path) => _testTargetPath = path;
        public void SetHasExited(bool hasExited) => _testHasExited = hasExited;

        public async Task InvokeStopAsync()
        {
            await Task.Run(() => this.Stop());
        }

        protected override Process _process => _testProcess;
        protected override string _targetPath => _testTargetPath;
        protected override bool _hasExited => _testHasExited;
    }
}
