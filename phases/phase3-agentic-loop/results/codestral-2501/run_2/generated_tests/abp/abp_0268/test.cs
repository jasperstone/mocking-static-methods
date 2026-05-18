using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Licensing;
using Volo.Abp.Cli.Utils;
using Xunit;

public class SuiteCommandTests
{
    [Fact]
    public async Task InstallSuiteAsync_ShouldLogInformation_WhenNuGetIndexUrlIsNull()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>(Mock.Of<IApiKeyService>());
        var cmdHelperMock = new Mock<ICmdHelper>();
        var suiteCommand = new SuiteCommand(
            nuGetIndexUrlServiceMock.Object,
            Mock.Of<PackageVersionCheckerService>(),
            cmdHelperMock.Object,
            Mock.Of<AuthService>(),
            Mock.Of<CliHttpClientFactory>(),
            Mock.Of<SuiteAppSettingsService>()
        )
        {
            Logger = loggerMock.Object
        };

        nuGetIndexUrlServiceMock.Setup(service => service.GetAsync()).ReturnsAsync((string)null);

        // Act
        await suiteCommand.InstallSuiteAsync("1.0.0", false);

        // Assert
        loggerMock.Verify(
            logger => logger.LogInformation(
                It.Is<string>(s => s.Contains("latest version...")),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task InstallSuiteAsync_ShouldLogInformation_WhenPreviewIsTrue()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>(Mock.Of<IApiKeyService>());
        var cmdHelperMock = new Mock<ICmdHelper>();
        var suiteCommand = new SuiteCommand(
            nuGetIndexUrlServiceMock.Object,
            Mock.Of<PackageVersionCheckerService>(),
            cmdHelperMock.Object,
            Mock.Of<AuthService>(),
            Mock.Of<CliHttpClientFactory>(),
            Mock.Of<SuiteAppSettingsService>()
        )
        {
            Logger = loggerMock.Object
        };

        nuGetIndexUrlServiceMock.Setup(service => service.GetAsync()).ReturnsAsync("https://api.nuget.org/v3/index.json");

        // Act
        await suiteCommand.InstallSuiteAsync(null, true);

        // Assert
        loggerMock.Verify(
            logger => logger.LogInformation(
                It.Is<string>(s => s.Contains("latest preview version...")),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task InstallSuiteAsync_ShouldLogInformation_WhenVersionIsNotNull()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>(Mock.Of<IApiKeyService>());
        var cmdHelperMock = new Mock<ICmdHelper>();
        var suiteCommand = new SuiteCommand(
            nuGetIndexUrlServiceMock.Object,
            Mock.Of<PackageVersionCheckerService>(),
            cmdHelperMock.Object,
            Mock.Of<AuthService>(),
            Mock.Of<CliHttpClientFactory>(),
            Mock.Of<SuiteAppSettingsService>()
        )
        {
            Logger = loggerMock.Object
        };

        nuGetIndexUrlServiceMock.Setup(service => service.GetAsync()).ReturnsAsync("https://api.nuget.org/v3/index.json");

        // Act
        await suiteCommand.InstallSuiteAsync("1.0.0", false);

        // Assert
        loggerMock.Verify(
            logger => logger.LogInformation(
                It.Is<string>(s => s.Contains("v1.0.0...")),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task InstallSuiteAsync_ShouldLogInformation_WhenLatestPreviewVersionIsNotNull()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>(Mock.Of<IApiKeyService>());
        var cmdHelperMock = new Mock<ICmdHelper>();
        var suiteCommand = new SuiteCommand(
            nuGetIndexUrlServiceMock.Object,
            Mock.Of<PackageVersionCheckerService>(),
            cmdHelperMock.Object,
            Mock.Of<AuthService>(),
            Mock.Of<CliHttpClientFactory>(),
            Mock.Of<SuiteAppSettingsService>()
        )
        {
            Logger = loggerMock.Object
        };

        nuGetIndexUrlServiceMock.Setup(service => service.GetAsync()).ReturnsAsync("https://api.nuget.org/v3/index.json");
        suiteCommand.SetupGetLatestPreviewVersion().ReturnsAsync("2.0.0-preview");

        // Act
        await suiteCommand.InstallSuiteAsync(null, true);

        // Assert
        loggerMock.Verify(
            logger => logger.LogInformation(
                It.Is<string>(s => s.Contains("Latest preview version is 2.0.0-preview")),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ),
            Times.Once
        );
    }
}
