using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public void StartSuite_LogsWarning_WhenAbpSuiteIsNotInstalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var abpNuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();
            var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var authServiceMock = new Mock<AuthService>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();

            var suiteCommand = new SuiteCommand(
                abpNuGetIndexUrlServiceMock.Object,
                packageVersionCheckerServiceMock.Object,
                cmdHelperMock.Object,
                authServiceMock.Object,
                cliHttpClientFactoryMock.Object,
                suiteAppSettingsServiceMock.Object
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            var startSuiteMethod = typeof(SuiteCommand).GetMethod("StartSuite", BindingFlags.NonPublic | BindingFlags.Instance);
            startSuiteMethod.Invoke(suiteCommand, null);

            // Assert
            loggerMock.Verify(l => l.LogWarning("ABP Suite is not installed! To install it you can run the command: \"abp suite install\""), Times.Once);
        }

        [Fact]
        public void StartSuite_LogsWarning_WhenCheckingAbpSuiteInstalledStatusFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var abpNuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();
            var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var authServiceMock = new Mock<AuthService>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();

            var suiteCommand = new SuiteCommand(
                abpNuGetIndexUrlServiceMock.Object,
                packageVersionCheckerServiceMock.Object,
                cmdHelperMock.Object,
                authServiceMock.Object,
                cliHttpClientFactoryMock.Object,
                suiteAppSettingsServiceMock.Object
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            var startSuiteMethod = typeof(SuiteCommand).GetMethod("StartSuite", BindingFlags.NonPublic | BindingFlags.Instance);
            startSuiteMethod.Invoke(suiteCommand, null);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Couldn't check ABP Suite installed status: "), Times.Once);
        }
    }
}
