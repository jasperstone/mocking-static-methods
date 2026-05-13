using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Core;
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
            var nuGetIndexUrlServiceMock = new Mock<IAbpNuGetIndexUrlService>();
            nuGetIndexUrlServiceMock.Setup(s => s.GetAsync()).ReturnsAsync("https://api.nuget.org/v3/index.json");
            var cmdHelperMock = new Mock<ICmdHelper>();
            var suiteCommand = new SuiteCommand(nuGetIndexUrlServiceMock.Object, null, cmdHelperMock.Object, null, null, null);
            suiteCommand.Logger = loggerMock.Object;

            // Act
            await suiteCommand.ExecuteAsync(new CommandLineArgs { Target = "install" });

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteAsync_InstallSuiteAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var nuGetIndexUrlServiceMock = new Mock<IAbpNuGetIndexUrlService>();
            nuGetIndexUrlServiceMock.Setup(s => s.GetAsync()).ReturnsAsync("https://api.nuget.org/v3/index.json");
            var cmdHelperMock = new Mock<ICmdHelper>();
            var suiteCommand = new SuiteCommand(nuGetIndexUrlServiceMock.Object, null, cmdHelperMock.Object, null, null, null);
            suiteCommand.Logger = loggerMock.Object;

            // Act
            await suiteCommand.ExecuteAsync(new CommandLineArgs { Target = "install", Options = new Dictionary<string, string> { { "version", "1.0.0" } } });

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
