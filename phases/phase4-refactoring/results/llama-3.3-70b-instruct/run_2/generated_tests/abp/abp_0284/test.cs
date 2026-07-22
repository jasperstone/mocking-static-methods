using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public void StartSuite_LogsWarning_WhenAbpSuiteIsNotInstalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();
            var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
            var authServiceMock = new Mock<AuthService>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();
            var globalToolHelperMock = new Mock<GlobalToolHelper>();
            globalToolHelperMock.Setup(x => x.IsGlobalToolInstalled("abp-suite")).Returns(false);

            var suiteCommand = new SuiteCommand(
                nuGetIndexUrlServiceMock.Object,
                packageVersionCheckerServiceMock.Object,
                cmdHelperMock.Object,
                authServiceMock.Object,
                cliHttpClientFactoryMock.Object,
                suiteAppSettingsServiceMock.Object
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            var result = suiteCommand.StartSuite();

            // Assert
            loggerMock.Verify(x => x.LogWarning("ABP Suite is not installed! To install it you can run the command: \"abp suite install\""), Times.Once);
        }

        [Fact]
        public void StartSuite_LogsWarning_WhenCheckingAbpSuiteInstalledStatusFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();
            var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
            var authServiceMock = new Mock<AuthService>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();
            var globalToolHelperMock = new Mock<GlobalToolHelper>();
            globalToolHelperMock.Setup(x => x.IsGlobalToolInstalled("abp-suite")).Throws(new Exception("Test exception"));

            var suiteCommand = new SuiteCommand(
                nuGetIndexUrlServiceMock.Object,
                packageVersionCheckerServiceMock.Object,
                cmdHelperMock.Object,
                authServiceMock.Object,
                cliHttpClientFactoryMock.Object,
                suiteAppSettingsServiceMock.Object
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            var result = suiteCommand.StartSuite();

            // Assert
            loggerMock.Verify(x => x.LogWarning("Couldn't check ABP Suite installed status: Test exception"), Times.Once);
        }
    }
}
