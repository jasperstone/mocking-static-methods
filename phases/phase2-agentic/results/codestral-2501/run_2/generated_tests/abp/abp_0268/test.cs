using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Utils;
using Xunit;

public class SuiteCommandTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldLogInformation_WhenInstallingSuite()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>(Mock.Of<IApiKeyService>());
        var cmdHelperMock = new Mock<ICmdHelper>();
        var authServiceMock = new Mock<AuthService>();
        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();

        var suiteCommand = new SuiteCommand(
            nuGetIndexUrlServiceMock.Object,
            null,
            cmdHelperMock.Object,
            authServiceMock.Object,
            cliHttpClientFactoryMock.Object,
            suiteAppSettingsServiceMock.Object)
        {
            Logger = loggerMock.Object
        };

        var commandLineArgs = new CommandLineArgs
        {
            Target = "install",
            Options = new CommandLineOptions()
        };

        nuGetIndexUrlServiceMock.Setup(x => x.GetAsync()).ReturnsAsync("https://api.nuget.org/v3/index.json");

        // Act
        await suiteCommand.ExecuteAsync(commandLineArgs);

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(
                It.Is<string>(s => s.Contains("latest version...")),
                It.IsAny<object[]>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogInformation_WhenInstallingPreviewSuite()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>(Mock.Of<IApiKeyService>());
        var cmdHelperMock = new Mock<ICmdHelper>();
        var authServiceMock = new Mock<AuthService>();
        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();

        var suiteCommand = new SuiteCommand(
            nuGetIndexUrlServiceMock.Object,
            null,
            cmdHelperMock.Object,
            authServiceMock.Object,
            cliHttpClientFactoryMock.Object,
            suiteAppSettingsServiceMock.Object)
        {
            Logger = loggerMock.Object
        };

        var commandLineArgs = new CommandLineArgs
        {
            Target = "install",
            Options = new CommandLineOptions()
            {
                { Options.Preview.Short, "true" }
            }
        };

        nuGetIndexUrlServiceMock.Setup(x => x.GetAsync()).ReturnsAsync("https://api.nuget.org/v3/index.json");

        // Act
        await suiteCommand.ExecuteAsync(commandLineArgs);

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(
                It.Is<string>(s => s.Contains("latest preview version...")),
                It.IsAny<object[]>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogInformation_WhenInstallingSpecificVersionSuite()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>(Mock.Of<IApiKeyService>());
        var cmdHelperMock = new Mock<ICmdHelper>();
        var authServiceMock = new Mock<AuthService>();
        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();

        var suiteCommand = new SuiteCommand(
            nuGetIndexUrlServiceMock.Object,
            null,
            cmdHelperMock.Object,
            authServiceMock.Object,
            cliHttpClientFactoryMock.Object,
            suiteAppSettingsServiceMock.Object)
        {
            Logger = loggerMock.Object
        };

        var commandLineArgs = new CommandLineArgs
        {
            Target = "install",
            Options = new CommandLineOptions()
            {
                { Options.Version.Short, "1.0.0" }
            }
        };

        nuGetIndexUrlServiceMock.Setup(x => x.GetAsync()).ReturnsAsync("https://api.nuget.org/v3/index.json");

        // Act
        await suiteCommand.ExecuteAsync(commandLineArgs);

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(
                It.Is<string>(s => s.Contains("v1.0.0...")),
                It.IsAny<object[]>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogInformation_WhenLatestPreviewVersionIsFound()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>(Mock.Of<IApiKeyService>());
        var cmdHelperMock = new Mock<ICmdHelper>();
        var authServiceMock = new Mock<AuthService>();
        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();

        var suiteCommand = new SuiteCommand(
            nuGetIndexUrlServiceMock.Object,
            null,
            cmdHelperMock.Object,
            authServiceMock.Object,
            cliHttpClientFactoryMock.Object,
            suiteAppSettingsServiceMock.Object)
        {
            Logger = loggerMock.Object
        };

        var commandLineArgs = new CommandLineArgs
        {
            Target = "install",
            Options = new CommandLineOptions()
            {
                { Options.Preview.Short, "true" }
            }
        };

        nuGetIndexUrlServiceMock.Setup(x => x.GetAsync()).ReturnsAsync("https://api.nuget.org/v3/index.json");
        suiteCommand.SetupGetLatestPreviewVersion().ReturnsAsync("1.0.0-preview");

        // Act
        await suiteCommand.ExecuteAsync(commandLineArgs);

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(
                It.Is<string>(s => s.Contains("Latest preview version is 1.0.0-preview")),
                It.IsAny<object[]>()
            ),
            Times.Once
        );
    }
}
