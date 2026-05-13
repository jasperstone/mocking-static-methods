using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.DependencyInjection;
using Volo.Abp.NuGet;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.Commands
{
    public class SuiteCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_Logs_Latest_Preview_Version()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var logExtensionMock = new Mock<LoggerExtensions>();
            var nugetIndexUrlServiceMock = new Mock<INuGetIndexUrlService>();
            var nugetPackageManagerMock = new Mock<INuGetPackageManager>();
            var suiteCommand<TLoggerExtensions> = new SuiteCommand(
                nugetIndexUrlServiceMock.Object,
                nugetPackageManagerMock.Object,
                null,
                null,
                null);

            nugetIndexUrlServiceMock
                .Setup(service => service.GetAsync())
                .ReturnsAsync("https://fake.nuget.source");

            nugetPackageManagerMock
                .Setup(manager => manager.GetLatestVersionAsync(It.IsAny<string>()))
                .ReturnsAsync("1.2.3-preview");

            suiteCommand.Logger = loggerMock.Object;
            suiteCommand.SuitePackageName = "Volo.Abp.Suite";
            suiteCommand.CmdHelper = new TestCmdHelper();

            var preview = true;
            var version = (string)null;

            // Act
            await suiteCommand.ExecuteAsync(preview, version, false);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation("Latest preview version is 1.2.3-preview"),
                Times.Once);
        }

        private class TestCmdHelper : ICmdHelper, ITransientDependency
        {
            public void RunCmd(string cmd, out int exitCode)
            {
                exitCode = 1;
            }
        }
    }
}
