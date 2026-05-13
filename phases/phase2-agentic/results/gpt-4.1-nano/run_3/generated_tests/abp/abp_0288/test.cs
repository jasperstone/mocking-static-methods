using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests
{
    public class SuiteCommandTests
    {
        private readonly Mock<ILogger<SuiteCommand>> _loggerMock;
        private readonly Mock<ICmdHelper> _cmdHelperMock;
        private readonly Mock<SuiteAppSettingsService> _suiteAppSettingsServiceMock;
        private readonly Mock<CliHttpClientFactory> _httpClientFactoryMock;
        private readonly SuiteCommand _suiteCommand;

        public SuiteCommandTests()
        {
            _loggerMock = new Mock<ILogger<SuiteCommand>>();
            _cmdHelperMock = new Mock<ICmdHelper>();
            _suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();
            _httpClientFactoryMock = new Mock<CliHttpClientFactory>();

            _suiteCommand = new SuiteCommand(
                null,
                null,
                _cmdHelperMock.Object,
                null,
                _httpClientFactoryMock.Object,
                _suiteAppSettingsServiceMock.Object
            )
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public void KillSuite_ShouldLogInformation_ForEachProcess()
        {
            // Arrange
            var mockProcess1 = new Mock<Process>();
            var mockProcess2 = new Mock<Process>();
            var processes = new List<Process> { mockProcess1.Object, mockProcess2.Object };

            // Setup GetProcessesRelatedWithSuite to return mock processes
            var suiteCommand = new PrivateObject(_suiteCommand);
            suiteCommand.SetFieldOrProperty("GetProcessesRelatedWithSuite", () => processes);

            // Act
            suiteCommand.Invoke("KillSuite");

            // Assert
            _loggerMock.Verify(x => x.LogInformation("Suite closed."), Times.Exactly(2));
            mockProcess1.Verify(p => p.Kill(), Times.Once);
            mockProcess2.Verify(p => p.Kill(), Times.Once);
        }

        [Fact]
        public void KillSuite_ShouldLogError_WhenExceptionThrown()
        {
            // Arrange
            var mockProcess = new Mock<Process>();
            mockProcess.Setup(p => p.Kill()).Throws(new Exception("Error"));

            var processes = new List<Process> { mockProcess.Object };
            var suiteCommand = new PrivateObject(_suiteCommand);
            suiteCommand.SetFieldOrProperty("GetProcessesRelatedWithSuite", () => processes);

            // Act
            suiteCommand.Invoke("KillSuite");

            // Assert
            _loggerMock.Verify(x => x.LogInformation(It.Is<string>(s => s.StartsWith("Cannot close Suite."))), Times.Once);
        }

        [Fact]
        public void LogInformation_ShouldBeCalledOnLogInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                null, null, _cmdHelperMock.Object, null, _httpClientFactoryMock.Object, _suiteAppSettingsServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            loggerMock.Object.LogInformation("Test message");

            // Assert
            loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Test message")),
                null,
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }
    }
}
