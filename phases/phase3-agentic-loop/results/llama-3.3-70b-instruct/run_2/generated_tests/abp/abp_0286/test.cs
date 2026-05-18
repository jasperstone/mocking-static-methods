using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Core;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public void StartSuite_LogsError_WhenPortIsInUse()
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
                suiteAppSettingsServiceMock.Object);
            suiteCommand.Logger = loggerMock.Object;
            suiteCommand._abpSuitePort = 3000;

            // Act
            var process = suiteCommand.StartSuite();

            // Assert
            loggerMock.Verify(x => x.LogError(It.IsAny<string>()), Times.Once);
        }
    }
}
