using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        private readonly Mock<ILogger<SuiteCommand>> _loggerMock;
        private readonly SuiteCommand _suiteCommand;

        public SuiteCommandTests()
        {
            _loggerMock = new Mock<ILogger<SuiteCommand>>();
            var mockDependencies = new MockDependencies();
            _suiteCommand = new SuiteCommand(
                mockDependencies.NuGetIndexUrlService,
                mockDependencies.PackageVersionCheckerService,
                mockDependencies.CmdHelper,
                mockDependencies.AuthService,
                mockDependencies.CliHttpClientFactory,
                mockDependencies.SuiteAppSettingsService
            )
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task InstallSuite_WhenSuccessful_LogsSuccessMessages()
        {
            // Arrange
            var mockDependencies = _suiteCommand as MockDependencies;
            mockDependencies.CmdHelper.Setup(cmd => cmd.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny))
                .Returns(Task.FromResult(0)); // Simulate successful command execution

            // Act
            await _suiteCommand.ExecuteAsync(new CommandLineArgs());

            // Assert
            _loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s.Contains("ABP Suite has been successfully installed.")),
                    It.IsAny<Exception>(),
                    It.IsAny<ILoggerLogDefine>(),
                    It.IsAny<Func<string, Exception, object, string>>()
                ), Times.Once);

            _loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s.Contains("You can run it with the CLI command \"abp suite\"")),
                    It.IsAny<Exception>(),
                    It.IsAny<ILoggerLogDefine>(),
                    It.IsAny<Func<string, Exception, object, string>>()
                ), Times.Once);
        }

        [Fact]
        public async Task InstallSuite_WhenFailed_LogsErrorMessage()
        {
            // Arrange
            var mockDependencies = _suiteCommand as MockDependencies;
            mockDependencies.CmdHelper.Setup(cmd => cmd.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny))
                .Returns(Task.FromResult(1)); // Simulate failed command execution

            // Act
            await _suiteCommand.ExecuteAsync(new CommandLineArgs());

            // Assert
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.Is<string>(s => s.Contains("Couldn't install ABP Suite.")),
                    It.IsAny<Exception>(),
                    It.IsAny<ILoggerLogDefine>(),
                    It.IsAny<Func<string, Exception, object, string>>()
                ), Times.Once);
        }

        private class MockDependencies
        {
            public Mock<AbpNuGetIndexUrlService> NuGetIndexUrlService { get; } = new Mock<AbpNuGetIndexUrlService>();
            public Mock<PackageVersionCheckerService> PackageVersionCheckerService { get; } = new Mock<PackageVersionCheckerService>();
            public Mock<ICmdHelper> CmdHelper { get; } = new Mock<ICmdHelper>();
            public Mock<AuthService> AuthService { get; } = new Mock<AuthService>();
            public Mock<CliHttpClientFactory> CliHttpClientFactory { get; } = new Mock<CliHttpClientFactory>();
            public Mock<SuiteAppSettingsService> SuiteAppSettingsService { get; } = new Mock<SuiteAppSettingsService>();
        }
    }
}
