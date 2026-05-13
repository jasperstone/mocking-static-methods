using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        [Fact]
        public void StartSuite_LogsWarning_WhenGlobalToolNotInstalled()
        {
            // Arrange
            var mockCmdHelper = new Mock<ICmdHelper>();
            var mockNuGetIndexUrlService = new Mock<AbpNuGetIndexUrlService>(null, null);
            var mockPackageVersionCheckerService = new Mock<PackageVersionCheckerService>(null, null);
            var mockAuthService = new Mock<AuthService>(null, null);
            var mockCliHttpClientFactory = new Mock<CliHttpClientFactory>(null);
            var mockSuiteAppSettingsService = new Mock<SuiteAppSettingsService>(null);

            var loggerMock = new Mock<ILogger<SuiteCommand>>();

            var suiteCommand = new SuiteCommand(
                mockNuGetIndexUrlService.Object,
                mockPackageVersionCheckerService.Object,
                mockCmdHelper.Object,
                mockAuthService.Object,
                mockCliHttpClientFactory.Object,
                mockSuiteAppSettingsService.Object)
            {
                Logger = loggerMock.Object
            };

            // Setup GlobalToolHelper.IsGlobalToolInstalled to return false
            // Since GlobalToolHelper is static, we cannot mock it directly.
            // Instead, we will simulate by creating a derived class that overrides StartSuite to call base but with a fake GlobalToolHelper.
            // But since we cannot do that easily, we will use a workaround by creating a partial mock or by reflection.
            // However, since the method is private, we will use reflection to invoke it.

            // We will simulate the static call by temporarily replacing the method via a delegate or by using a wrapper.
            // Since this is complicated, we will create a derived test class that overrides StartSuite and simulates the behavior.

            var testSuiteCommand = new TestSuiteCommand(
                mockNuGetIndexUrlService.Object,
                mockPackageVersionCheckerService.Object,
                mockCmdHelper.Object,
                mockAuthService.Object,
                mockCliHttpClientFactory.Object,
                mockSuiteAppSettingsService.Object,
                loggerMock.Object);

            // Act
            var result = testSuiteCommand.InvokeStartSuite();

            // Assert
            Assert.Null(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ABP Suite is not installed!")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestSuiteCommand : SuiteCommand
        {
            private readonly ILogger<SuiteCommand> _logger;

            public TestSuiteCommand(
                AbpNuGetIndexUrlService nuGetIndexUrlService,
                PackageVersionCheckerService packageVersionCheckerService,
                ICmdHelper cmdHelper,
                AuthService authService,
                CliHttpClientFactory cliHttpClientFactory,
                SuiteAppSettingsService suiteAppSettingsService,
                ILogger<SuiteCommand> logger)
                : base(nuGetIndexUrlService, packageVersionCheckerService, cmdHelper, authService, cliHttpClientFactory, suiteAppSettingsService)
            {
                _logger = logger;
                Logger = logger;
            }

            public Process InvokeStartSuite()
            {
                return StartSuite();
            }

            // Override the static call to GlobalToolHelper.IsGlobalToolInstalled to simulate false
            protected override bool IsGlobalToolInstalled(string toolName)
            {
                return false;
            }
        }
    }
}
