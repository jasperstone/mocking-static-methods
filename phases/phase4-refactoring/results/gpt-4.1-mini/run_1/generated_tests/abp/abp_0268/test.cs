using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        private class TestAbpNuGetIndexUrlService : AbpNuGetIndexUrlService
        {
            public override Task<string> GetAsync()
            {
                return Task.FromResult("http://nuget.index.url");
            }
        }

        [Fact]
        public async Task InstallSuiteAsync_LogsInformationOnLatestPreviewVersion()
        {
            // Arrange
            var nugetIndexUrlService = new TestAbpNuGetIndexUrlService();
            var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var authServiceMock = new Mock<AuthService>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();

            var loggerMock = new Mock<ILogger<SuiteCommand>>();

            var suiteCommand = new SuiteCommand(
                nugetIndexUrlService,
                packageVersionCheckerServiceMock.Object,
                cmdHelperMock.Object,
                authServiceMock.Object,
                cliHttpClientFactoryMock.Object,
                suiteAppSettingsServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Setup command line args with Target = "install" and preview option set
            var options = new Volo.Abp.Cli.Args.AbpCommandLineOptions();
            options.Add("p", "true"); // Using short option "p" for preview

            var args = new CommandLineArgs(null, "install");

            // Use reflection to set the readonly Options property
            var optionsProperty = typeof(CommandLineArgs).GetProperty("Options");
            optionsProperty.SetValue(args, options);

            // Act
            await suiteCommand.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Latest preview version is")),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
