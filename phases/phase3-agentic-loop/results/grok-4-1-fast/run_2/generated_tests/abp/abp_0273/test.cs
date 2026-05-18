using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class SuiteCommandTests
{
    private readonly Mock<ILogger<SuiteCommand>> _loggerMock;

    public SuiteCommandTests()
    {
        _loggerMock = new Mock<ILogger<SuiteCommand>>();
    }

    [Fact]
    public void ShowSuiteManualInstallCommand_ShouldLogInformationWithCorrectMessage()
    {
        // Arrange
        var suiteCommand = CreateSuiteCommand();

        // Act
        ((dynamic)suiteCommand).ShowSuiteManualInstallCommand();

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("dotnet tool install -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private SuiteCommand CreateSuiteCommand()
    {
        var mockNuGetService = new Mock<AbpNuGetIndexUrlService>();
        var mockCmdHelper = new Mock<ICmdHelper>();
        mockCmdHelper.SetupAllProperties();
        var mockAuthService = new Mock<AuthService>();
        var mockHttpFactory = new Mock<CliHttpClientFactory>();
        var mockSuiteSettings = new Mock<SuiteAppSettingsService>();

        var suiteCommand = new SuiteCommand(
            mockNuGetService.Object,
            null!, // PackageVersionCheckerService - using null since not used in ShowSuiteManualInstallCommand
            mockCmdHelper.Object,
            mockAuthService.Object,
            mockHttpFactory.Object,
            mockSuiteSettings.Object
        )
        {
            Logger = _loggerMock.Object
        };

        return suiteCommand;
    }
}
