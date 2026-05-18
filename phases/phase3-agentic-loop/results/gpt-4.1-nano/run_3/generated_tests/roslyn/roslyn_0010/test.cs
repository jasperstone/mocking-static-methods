using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;

namespace Microsoft.CodeAnalysis.MSBuild.Tests
{
    public class BuildHostProcessManagerTests
    {
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<ILogger> _loggerMock;

        public BuildHostProcessManagerTests()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger>();
            _loggerFactoryMock.Setup(f => f.CreateLogger<BuildHostProcessManager>()).Returns(_loggerMock.Object);
        }

        [Fact]
        public void LogProcessFailure_ShouldLogError_WhenLoggerIsNotNull()
        {
            // Arrange
            var manager = new BuildHostProcessManager(globalMSBuildProperties: null, loggerFactory: _loggerFactoryMock.Object);
            var loggerMock = new Mock<ILogger>();
            var instance = (dynamic)Activator.CreateInstance(typeof(BuildHostProcessManager), new object[] { null, null, _loggerFactoryMock.Object });
            // Set private _logger to mock
            var loggerField = typeof(BuildHostProcessManager).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(instance, loggerMock.Object);
            // Set private _processLogMessages
            var processLogMessagesField = typeof(BuildHostProcessManager).GetField("_processLogMessages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            processLogMessagesField.SetValue(instance, new System.Text.StringBuilder("log message"));
            // Set private _process with HasExited = false, ExitCode = 1
            var processMock = new Mock<System.Diagnostics.Process>();
            processMock.Setup(p => p.HasExited).Returns(false);
            processMock.Setup(p => p.ExitCode).Returns(1);
            var processField = typeof(BuildHostProcessManager).GetField("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            processField.SetValue(instance, processMock.Object);

            // Act
            instance.LogProcessFailure();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void LogProcessFailure_ShouldNotLogError_WhenLoggerIsNull()
        {
            // Arrange
            var manager = new BuildHostProcessManager(globalMSBuildProperties: null, loggerFactory: null);
            var instance = (dynamic)Activator.CreateInstance(typeof(BuildHostProcessManager), new object[] { null, null, null });
            // Set private _processLogMessages
            var processLogMessagesField = typeof(BuildHostProcessManager).GetField("_processLogMessages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            processLogMessagesField.SetValue(instance, new System.Text.StringBuilder("log message"));
            // Set private _process with HasExited = false, ExitCode = 1
            var processMock = new Mock<System.Diagnostics.Process>();
            processMock.Setup(p => p.HasExited).Returns(false);
            processMock.Setup(p => p.ExitCode).Returns(1);
            var processField = typeof(BuildHostProcessManager).GetField("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            processField.SetValue(instance, processMock.Object);

            // Act
            instance.LogProcessFailure();

            // Since _logger is null, no exception should be thrown and nothing should be logged
        }
    }
}
