using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Services;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_InstallSuiteIfNotInstalledAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var abpNuGetIndexUrlServiceMock = new Mock<IAbpNuGetIndexUrlService>();
            abpNuGetIndexUrlServiceMock.Setup(s => s.GetAsync()).ReturnsAsync("https://api.abp.io/api/abp/nuget/index-url");
            var suiteCommand = new SuiteCommand(abpNuGetIndexUrlServiceMock.Object, null, null, null, null, null);
            suiteCommand.Logger = loggerMock.Object;

            // Act
            await suiteCommand.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteAsync_InstallSuiteAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var abpNuGetIndexUrlServiceMock = new Mock<IAbpNuGetIndexUrlService>();
            abpNuGetIndexUrlServiceMock.Setup(s => s.GetAsync()).ReturnsAsync("https://api.abp.io/api/abp/nuget/index-url");
            var suiteCommand = new SuiteCommand(abpNuGetIndexUrlServiceMock.Object, null, null, null, null, null);
            suiteCommand.Logger = loggerMock.Object;

            // Act
            await suiteCommand.ExecuteAsync(new CommandLineArgs { Target = "install" });

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
