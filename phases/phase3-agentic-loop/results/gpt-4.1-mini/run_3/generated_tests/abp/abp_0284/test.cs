using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class SuiteCommandTests
{
    [Fact]
    public void StartSuite_LogsWarning_WhenSuiteNotInstalled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var cmdHelperMock = new Mock<ICmdHelper>();
        var nuGetIndexUrlService = new Mock<AbpNuGetIndexUrlService>(null, null);
        var packageVersionCheckerService = new Mock<PackageVersionCheckerService>(null, null);
        var authService = new Mock<AuthService>(null, null);
        var cliHttpClientFactory = new Mock<CliHttpClientFactory>(null);
        var suiteAppSettingsService = new Mock<SuiteAppSettingsService>(null);

        var suiteCommand = new SuiteCommand(
            nuGetIndexUrlService.Object,
            packageVersionCheckerService.Object,
            cmdHelperMock.Object,
            authService.Object,
            cliHttpClientFactory.Object,
            suiteAppSettingsService.Object
        )
        {
            Logger = loggerMock.Object
        };

        // Setup GlobalToolHelper.IsGlobalToolInstalled to return false
        // We cannot mock static methods easily, so we will simulate by replacing the method with a delegate or by other means.
        // Since we cannot do that here, we will create a derived class to override the method.

        var testSuiteCommand = new TestSuiteCommand(
            nuGetIndexUrlService.Object,
            packageVersionCheckerService.Object,
            cmdHelperMock.Object,
            authService.Object,
            cliHttpClientFactory.Object,
            suiteAppSettingsService.Object,
            loggerMock.Object
        );

        // Act
        var result = testSuiteCommand.StartSuite();

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

        protected override bool IsGlobalToolInstalled(string toolName)
        {
            // Simulate that the tool is not installed
            return false;
        }
    }
}
