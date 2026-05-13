using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_InstallSuiteIfNotInstalledAsync_LogInformationCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                Mock.Of<AbpNuGetIndexUrlService>(),
                Mock.Of<PackageVersionCheckerService>(),
                Mock.Of<ICmdHelper>(),
                Mock.Of<AuthService>(),
                Mock.Of<CliHttpClientFactory>(),
                Mock.Of<SuiteAppSettingsService>()
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            await suiteCommand.ExecuteAsync(new CommandLineArgs { Target = "" });

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteAsync_InstallSuiteAsync_LogInformationCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                Mock.Of<AbpNuGetIndexUrlService>(),
                Mock.Of<PackageVersionCheckerService>(),
                Mock.Of<ICmdHelper>(),
                Mock.Of<AuthService>(),
                Mock.Of<CliHttpClientFactory>(),
                Mock.Of<SuiteAppSettingsService>()
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            await suiteCommand.ExecuteAsync(new CommandLineArgs { Target = "install" });

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteAsync_UpdateSuiteAsync_LogInformationCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                Mock.Of<AbpNuGetIndexUrlService>(),
                Mock.Of<PackageVersionCheckerService>(),
                Mock.Of<ICmdHelper>(),
                Mock.Of<AuthService>(),
                Mock.Of<CliHttpClientFactory>(),
                Mock.Of<SuiteAppSettingsService>()
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            await suiteCommand.ExecuteAsync(new CommandLineArgs { Target = "update" });

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteAsync_RemoveSuite_LogInformationCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                Mock.Of<AbpNuGetIndexUrlService>(),
                Mock.Of<PackageVersionCheckerService>(),
                Mock.Of<ICmdHelper>(),
                Mock.Of<AuthService>(),
                Mock.Of<CliHttpClientFactory>(),
                Mock.Of<SuiteAppSettingsService>()
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            await suiteCommand.ExecuteAsync(new CommandLineArgs { Target = "remove" });

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
