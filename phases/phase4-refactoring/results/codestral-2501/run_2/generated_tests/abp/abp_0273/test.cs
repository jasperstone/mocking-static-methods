using Xunit;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Version;
using System.Threading.Tasks;

public class SuiteCommandTests
{
    [Fact]
    public async Task InstallSuiteAsync_LogsCorrectInformation_WhenInstallationFails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var cmdHelperMock = new Mock<ICmdHelper>();
        cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny)).Returns(1); // Simulate a failed installation

        var suiteCommand = new SuiteCommand(
            Mock.Of<AbpNuGetIndexUrlService>(),
            Mock.Of<PackageVersionCheckerService>(),
            cmdHelperMock.Object,
            Mock.Of<AuthService>(),
            Mock.Of<CliHttpClientFactory>(),
            Mock.Of<SuiteAppSettingsService>()
        )
        {
            Logger = loggerMock.Object
        };

        // Act
        await suiteCommand.InstallSuiteAsync(null, false);

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(
                It.Is<string>(s => s.Contains("dotnet tool install -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json"))
            ),
            Times.Once
        );
    }
}
