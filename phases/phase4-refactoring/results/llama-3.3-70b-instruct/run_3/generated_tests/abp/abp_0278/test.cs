using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public async Task Test_LogError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                new AbpNuGetIndexUrlService(new ApiKeyService()),
                new PackageVersionCheckerService(
                    new JsonSerializer(),
                    new RemoteServiceExceptionHandler(),
                    new CancellationTokenProvider(),
                    new ApiKeyService(),
                    new CliHttpClientFactory(new HttpClientFactory(), new CancellationTokenProvider())
                ),
                new CmdHelper(new AbpCliOptions()),
                new AuthService(
                    new IdentityModelAuthenticationService(),
                    new Logger<AuthService>(new LoggerFactory()),
                    new CancellationTokenProvider(),
                    new CliHttpClientFactory(new HttpClientFactory(), new CancellationTokenProvider()),
                    new RemoteServiceExceptionHandler(),
                    new JsonSerializer()
                ),
                new CliHttpClientFactory(new HttpClientFactory(), new CancellationTokenProvider()),
                new SuiteAppSettingsService(new CmdHelper(new AbpCliOptions()))
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            suiteCommand.ShowSuiteManualUpdateCommand();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Exactly(2));
        }
    }
}
