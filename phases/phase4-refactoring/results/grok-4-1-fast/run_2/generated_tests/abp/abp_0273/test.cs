using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Xunit;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Commands.Tests;

public class SuiteCommandTests
{
    private readonly Mock<ILogger<SuiteCommand>> _loggerMock;

    public SuiteCommandTests()
    {
        _loggerMock = new Mock<ILogger<SuiteCommand>>();
        _loggerMock.SetupAllProperties();
    }

    [Fact]
    public void ShowSuiteManualInstallCommand_ShouldLogSpecificInformationMessage()
    {
        // Arrange - Create minimal mocks for constructor dependencies
        var cmdHelperMock = new Mock<ICmdHelper>();
        var cliOptionsMock = new Mock<IOptionsSnapshot<AbpCliOptions>>();
        cliOptionsMock.Setup(x => x.Value).Returns(new AbpCliOptions());

        var cmdHelper = new CmdHelper(cliOptionsMock.Object);

        // Use reflection to create SuiteCommand instance and invoke private method
        var suiteCommand = new SuiteCommand(
            new FakeNuGetIndexUrlService(),
            new FakePackageVersionCheckerService(),
            cmdHelper,
            new FakeAuthService(),
            new FakeCliHttpClientFactory(),
            new FakeSuiteAppSettingsService()
        )
        {
            Logger = _loggerMock.Object
        };

        // Act - Invoke private method using reflection
        var method = typeof(SuiteCommand).GetMethod("ShowSuiteManualInstallCommand", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method?.Invoke(suiteCommand, null);

        // Assert - Verifies Logger.LogInformation extension call on line 333
        _loggerMock.Verify(
            x => x.LogInformation(
                It.Is<string>(s => 
                    s.Contains("dotnet tool install -g Volo.Abp.Suite") &&
                    s.Contains("nuget.abp.io") &&
                    s.Contains("<your-private-key>")
                )
            ),
            Times.Once
        );
    }
}

// Minimal fake implementations to satisfy constructor requirements
public class FakeNuGetIndexUrlService : Volo.Abp.Cli.Commands.Services.AbpNuGetIndexUrlService
{
    public FakeNuGetIndexUrlService(Volo.Abp.Cli.Http.CliHttpClientFactory httpClientFactory) : base(httpClientFactory) { }
}

public class FakePackageVersionCheckerService : Volo.Abp.Cli.Version.PackageVersionCheckerService
{
}

public class FakeAuthService : Volo.Abp.Cli.Auth.AuthService
{
}

public class FakeCliHttpClientFactory : Volo.Abp.Cli.Http.CliHttpClientFactory
{
}

public class FakeSuiteAppSettingsService : Volo.Abp.Cli.Commands.Services.SuiteAppSettingsService
{
}
