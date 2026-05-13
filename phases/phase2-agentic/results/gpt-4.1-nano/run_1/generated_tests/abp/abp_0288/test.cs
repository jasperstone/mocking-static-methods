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
        private readonly Mock<AbpNuGetIndexUrlService> _nuGetIndexUrlServiceMock;
        private readonly Mock<PackageVersionCheckerService> _packageVersionCheckerServiceMock;
        private readonly Mock<AuthService> _authServiceMock;
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<SuiteAppSettingsService> _suiteAppSettingsServiceMock;
        private readonly SuiteCommand _suiteCommand;

        public SuiteCommandTests()
        {
            _loggerMock = new Mock<ILogger<SuiteCommand>>();
            _cmdHelperMock = new Mock<ICmdHelper>();
            _nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();
            _packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
            _authServiceMock = new Mock<AuthService>();
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            _suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();

            _suiteCommand = new SuiteCommand(
                _nuGetIndexUrlServiceMock.Object,
                _packageVersionCheckerServiceMock.Object,
                _cmdHelperMock.Object,
                _authServiceMock.Object,
                _cliHttpClientFactoryMock.Object,
                _suiteAppSettingsServiceMock.Object
            )
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task KillSuite_ShouldLogInformation_WhenProcessKilled()
        {
            // Arrange
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.Kill());
            var processes = new List<Process> { processMock.Object };
            var processEnumerable = processes.AsEnumerable();

            // Mock GetProcessesRelatedWithSuite to return our process
            var suiteCommandType = typeof(SuiteCommand);
            var getProcessesMethod = suiteCommandType.GetMethod("GetProcessesRelatedWithSuite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var suiteCommandInstance = _suiteCommand;

            // Use reflection to invoke private method
            var killMethod = suiteCommandType.GetMethod("KillSuite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            await (Task)getProcessesMethod.Invoke(suiteCommandInstance, null);
            // Since KillSuite is private, invoke via reflection
            killMethod.Invoke(suiteCommandInstance, null);

            // Assert
            _loggerMock.Verify(x => x.LogInformation("Suite closed."), Times.AtLeastOnce);
        }

        [Fact]
        public async Task LogInformation_CalledOnExceptionInKillSuite()
        {
            // Arrange
            var suiteCommand = new Mock<SuiteCommand>(
                _nuGetIndexUrlServiceMock.Object,
                _packageVersionCheckerServiceMock.Object,
                _cmdHelperMock.Object,
                _authServiceMock.Object,
                _cliHttpClientFactoryMock.Object,
                _suiteAppSettingsServiceMock.Object
            )
            { CallBase = true };

            suiteCommand.Setup(s => s.GetProcessesRelatedWithSuite()).Throws(new Exception("Test exception"));

            // Act
            await Assert.ThrowsAsync<Exception>(() => suiteCommand.Object.KillSuite());

            // Assert
            suiteCommand.Verify(s => s.Logger.LogInformation("Cannot close Suite." + It.IsAny<string>()), Times.Once);
        }
    }
}
