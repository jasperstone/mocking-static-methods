using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class SuiteCommandTests
    {
        private readonly Mock<ILogger<SuiteCommand>> _loggerMock;
        private readonly Mock<ICmdHelper> _cmdHelperMock;
        private readonly Mock<SuiteAppSettingsService> _suiteAppSettingsServiceMock;
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly SuiteCommand _suiteCommand;

        public SuiteCommandTests()
        {
            _loggerMock = new Mock<ILogger<SuiteCommand>>();
            _cmdHelperMock = new Mock<ICmdHelper>();
            _suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();

            _suiteCommand = new SuiteCommand(
                new Mock<AbpNuGetIndexUrlService>().Object,
                new Mock<PackageVersionCheckerService>().Object,
                _cmdHelperMock.Object,
                new Mock<AuthService>().Object,
                _cliHttpClientFactoryMock.Object,
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
            var processMock1 = new Mock<Process>();
            var processMock2 = new Mock<Process>();
            var processes = new List<Process> { processMock1.Object, processMock2.Object };

            // Setup GetProcessesRelatedWithSuite to return mock processes
            var suiteCommandMock = new Mock<SuiteCommand>(
                new Mock<AbpNuGetIndexUrlService>().Object,
                new Mock<PackageVersionCheckerService>().Object,
                _cmdHelperMock.Object,
                new Mock<AuthService>().Object,
                _cliHttpClientFactoryMock.Object,
                _suiteAppSettingsServiceMock.Object
            )
            {
                CallBase = true
            };
            suiteCommandMock.Setup(c => c.GetProcessesRelatedWithSuite()).Returns(processes);

            // Act
            suiteCommandMock.Object.KillSuite();

            // Assert
            processMock1.Verify(p => p.Kill(), Times.Once);
            processMock2.Verify(p => p.Kill(), Times.Once);
            _loggerMock.Verify(
                l => l.LogInformation("Suite closed."),
                Times.Exactly(2)
            );
        }

        [Fact]
        public void KillSuite_ShouldLogInformation_WhenExceptionThrown()
        {
            // Arrange
            var processMock = new Mock<Process>();
            var processes = new List<Process> { processMock.Object };

            var suiteCommandMock = new Mock<SuiteCommand>(
                new Mock<AbpNuGetIndexUrlService>().Object,
                new Mock<PackageVersionCheckerService>().Object,
                _cmdHelperMock.Object,
                new Mock<AuthService>().Object,
                _cliHttpClientFactoryMock.Object,
                _suiteAppSettingsServiceMock.Object
            )
            {
                CallBase = true
            };
            suiteCommandMock.Setup(c => c.GetProcessesRelatedWithSuite()).Returns(processes);
            suiteCommandMock.Setup(p => p.Kill()).Throws(new Exception("Test exception"));

            // Act
            suiteCommandMock.Object.KillSuite();

            // Assert
            _loggerMock.Verify(
                l => l.LogInformation(It.Is<string>(s => s.StartsWith("Cannot close Suite."))),
                Times.Once
            );
        }
    }
}
