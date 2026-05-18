using Xunit;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Licensing;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        private readonly Mock<ILogger<SuiteCommand>> _loggerMock;
        private readonly SuiteCommand _suiteCommand;

        public SuiteCommandTests()
        {
            _loggerMock = new Mock<ILogger<SuiteCommand>>();
            _suiteCommand = new SuiteCommand(
                new AbpNuGetIndexUrlService(Mock.Of<IApiKeyService>()),
                new PackageVersionCheckerService(),
                Mock.Of<ICmdHelper>(),
                new AuthService(Mock.Of<IIdentityModelAuthenticationService>(), Mock.Of<ILogger<AuthService>>(), Mock.Of<ICancellationTokenProvider>(), Mock.Of<CliHttpClientFactory>(), Mock.Of<RemoteServiceExceptionHandler>(), Mock.Of<IJsonSerializer>()),
                new CliHttpClientFactory(Mock.Of<IHttpClientFactory>(), Mock.Of<ICancellationTokenProvider>()),
                new SuiteAppSettingsService(Mock.Of<CmdHelper>())
            )
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public void StartSuite_WhenAbpSuiteIsNotInstalled_LogsWarning()
        {
            // Arrange
            var toolCommandName = "abp-suite";
            var suitePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dotnet", "tools", toolCommandName + ".exe");
            if (File.Exists(suitePath))
            {
                File.Delete(suitePath);
            }

            // Act
            var result = _suiteCommand.StartSuite();

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning("ABP Suite is not installed! To install it you can run the command: \"abp suite install\""),
                Times.Once
            );
            Assert.Null(result);
        }
    }
}
