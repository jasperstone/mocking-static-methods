using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;

namespace Volo.Abp.Cli.Tests.Commands
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

            var suiteCommand = new SuiteCommand(
                nuGetIndexUrlServiceMock.Object,
                packageVersionCheckerServiceMock.Object,
                cmdHelperMock.Object,
                authServiceMock.Object,
                cliHttpClientFactoryMock.Object,
                suiteAppSettingsServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Simulate that the global tool is not installed
            GlobalToolHelper.IsGlobalToolInstalled = (toolName) => false;

            // Act
            var process = suiteCommand.StartSuite();

            // Assert
            loggerMock.Verify(
                logger => logger.LogWarning(
                    It.Is<string>(s => s.Contains("ABP Suite is not installed! To install it you can run the command: \"abp suite install\"")),
                    It.IsAny<Exception>()
                ),
                Times.Once
            );

            Assert.Null(process);
        }
    }
}
