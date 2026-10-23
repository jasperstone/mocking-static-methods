using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogInformationCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                Mock.Of<AbpNuGetIndexUrlService>(),
                Mock.Of<PackageVersionCheckerService>(),
                Mock.Of<ICmdHelper>(),
                Mock.Of<AuthService>(),
                Mock.Of<CliHttpClientFactory>(),
                Mock.Of<SuiteAppSettingsService>()
            );
            suiteCommand.Logger = loggerMock.Object;

            var commandLineArgs = new CommandLineArgs
            {
                Target = "",
                Options = new Dictionary<string, string>
                {
                    { "--preview", "" }
                }
            };

            // Act
            await suiteCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
