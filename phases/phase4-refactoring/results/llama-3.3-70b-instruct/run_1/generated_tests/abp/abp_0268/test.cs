using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogInformationCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>(new object[] { });
            var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>(new object[] { });
            var cmdHelperMock = new Mock<ICmdHelper>();
            var authServiceMock = new Mock<AuthService>(new object[] { });
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(new object[] { });
            var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>(new object[] { });
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
            await suiteCommand.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
