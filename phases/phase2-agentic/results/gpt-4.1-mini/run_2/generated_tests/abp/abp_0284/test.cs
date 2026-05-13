using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public void StartSuite_LogsWarning_WhenGlobalToolNotInstalled()
        {
            // Arrange
            var mockCmdHelper = new Mock<ICmdHelper>();
            var mockNuGetIndexUrlService = new Mock<AbpNuGetIndexUrlService>(null);
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
            // Instead, we can create a derived class to override StartSuite for testing or use reflection.
            // But since the method is private, we can use reflection to invoke it.
            // Alternatively, we can test the behavior by making StartSuite public for testing or internal with InternalsVisibleTo.
            // Here, we will use reflection to invoke StartSuite.

            // Act
            var startSuiteMethod = typeof(SuiteCommand).GetMethod("StartSuite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(startSuiteMethod);

            // To simulate GlobalToolHelper.IsGlobalToolInstalled returning false, we can temporarily replace the method using a delegate or shim.
            // Since we cannot do that easily here, we will create a derived class that overrides StartSuite to simulate the behavior.

            var testSuiteCommand = new TestSuiteCommand(
                mockNuGetIndexUrlService.Object,
                mockPackageVersionCheckerService.Object,
                mockCmdHelper.Object,
                mockAuthService.Object,
                mockCliHttpClientFactory.Object,
                mockSuiteAppSettingsService.Object,
                loggerMock.Object);

            var result = testSuiteCommand.InvokeStartSuite();

            // Assert
            Assert.Null(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ABP Suite is not installed! To install it you can run the command")),
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
