using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Volo.Abp.Cli.Args;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class SuiteCommandTests
{
    private readonly Mock<ILogger<SuiteCommand>> _mockLogger;

    public SuiteCommandTests()
    {
        _mockLogger = new Mock<ILogger<SuiteCommand>>();
    }

    [Fact]
    public void ShowSuiteManualInstallCommand_ShouldLogInformationWithCorrectMessage()
    {
        // Arrange
        var mockCmdHelper = new Mock<ICmdHelper>().Object;
        var suiteCommand = new SuiteCommand(
            nuGetIndexUrlService: new FakeNuGetIndexUrlService(),
            packageVersionCheckerService: new FakePackageVersionCheckerService(),
            cmdHelper: mockCmdHelper,
            authService: new FakeAuthService(),
            cliHttpClientFactory: new FakeCliHttpClientFactory(),
            suiteAppSettingsService: new FakeSuiteAppSettingsService()
        )
        {
            Logger = _mockLogger.Object
        };

        // Act
        suiteCommand.ShowSuiteManualInstallCommand();

        // Assert - verify the LogInformation extension call on line 333
        _mockLogger.Verify(
            x => x.LogInformation(
                It.Is<string>(msg => 
                    msg.Contains("dotnet tool install -g Volo.Abp.Suite") &&
                    msg.Contains("https://nuget.abp.io/<your-private-key>/v3/index.json")
                )
            ),
            Times.Once
        );
    }

    // Minimal fake implementations to satisfy constructor requirements
    private class FakeNuGetIndexUrlService : Volo.Abp.Cli.Commands.Services.AbpNuGetIndexUrlService
    {
        public FakeNuGetIndexUrlService(ILogger<AbpNuGetIndexUrlService> logger) : base(logger) { }
    }

    private class FakePackageVersionCheckerService : Volo.Abp.Cli.Commands.Services.PackageVersionCheckerService
    {
    }

    private class FakeCmdHelper : Volo.Abp.Cli.Utils.ICmdHelper
    {
        public void RunCmd(string cmd, string workingDirectory = null, Action<string> stdOutLineHandler = null, Action<string> stdErrLineHandler = null)
        {
        }

        public int RunCmd(string cmd, out int exitCode, string workingDirectory = null, Action<string> stdOutLineHandler = null, Action<string> stdErrLineHandler = null)
        {
            exitCode = 0;
            return 0;
        }
    }

    private class FakeAuthService : Volo.Abp.Cli.Auth.AuthService
    {
    }

    private class FakeCliHttpClientFactory : Volo.Abp.Cli.Http.CliHttpClientFactory
    {
    }

    private class FakeSuiteAppSettingsService : Volo.Abp.Cli.Commands.Services.SuiteAppSettingsService
    {
    }
}
