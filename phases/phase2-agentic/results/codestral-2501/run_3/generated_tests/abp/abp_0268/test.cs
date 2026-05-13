using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        private readonly Mock<ILogger<SuiteCommand>> _loggerMock;
        private readonly Mock<AbpNuGetIndexUrlService> _nuGetIndexUrlServiceMock;
        private readonly Mock<ICmdHelper> _cmdHelperMock;
        private readonly SuiteCommand _suiteCommand;

        public SuiteCommandTests()
        {
            _loggerMock = new Mock<ILogger<SuiteCommand>>();
            _nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>(MockBehavior.Strict);
            _cmdHelperMock = new Mock<ICmdHelper>();

            _suiteCommand = new SuiteCommand(
                _nuGetIndexUrlServiceMock.Object,
                null,
                _cmdHelperMock.Object,
                null,
                null,
                null)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task InstallSuiteAsync_ShouldLogInformation_WhenNuGetIndexUrlIsNull()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs(new[] { "suite", "install" });
            _nuGetIndexUrlServiceMock.Setup(x => x.GetAsync()).ReturnsAsync((string)null);

            // Act
            await _suiteCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("latest version...")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task InstallSuiteAsync_ShouldLogInformation_WhenLatestPreviewVersionIsNotNull()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs(new[] { "suite", "install", "--preview" });
            _nuGetIndexUrlServiceMock.Setup(x => x.GetAsync()).ReturnsAsync("https://api.nuget.org/v3/index.json");
            _cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny)).Returns(true);

            // Act
            await _suiteCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Latest preview version is")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
