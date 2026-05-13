using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
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
        public async Task InstallSuiteAsync_ShouldLogInformation_WhenNuGetIndexUrlIsNotNull()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs(new[] { "suite", "install" });
            _nuGetIndexUrlServiceMock.Setup(x => x.GetAsync()).ReturnsAsync("https://api.nuget.org/v3/index.json");
            _cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny)).Returns(true);

            // Act
            await _suiteCommand.InstallSuiteAsync(null, false);

            // Assert
            _loggerMock.Verify(x => x.LogInformation(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Func<It.IsAnyType, Exception, string>>(), It.IsAny<Exception>()), Times.Exactly(2));
        }

        [Fact]
        public async Task InstallSuiteAsync_ShouldReturn_WhenNuGetIndexUrlIsNull()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs(new[] { "suite", "install" });
            _nuGetIndexUrlServiceMock.Setup(x => x.GetAsync()).ReturnsAsync((string)null);

            // Act
            await _suiteCommand.InstallSuiteAsync(null, false);

            // Assert
            _cmdHelperMock.Verify(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny), Times.Never);
        }

        [Fact]
        public async Task InstallSuiteAsync_ShouldLogInformation_WhenPreviewIsTrueAndLatestPreviewVersionIsNull()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs(new[] { "suite", "install", "--preview" });
            _nuGetIndexUrlServiceMock.Setup(x => x.GetAsync()).ReturnsAsync("https://api.nuget.org/v3/index.json");
            _cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny)).Returns(true);

            // Act
            await _suiteCommand.InstallSuiteAsync(null, true);

            // Assert
            _loggerMock.Verify(x => x.LogInformation(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Func<It.IsAnyType, Exception, string>>(), It.IsAny<Exception>()), Times.Exactly(2));
        }

        [Fact]
        public async Task InstallSuiteAsync_ShouldLogInformation_WhenPreviewIsFalseAndVersionIsNull()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs(new[] { "suite", "install" });
            _nuGetIndexUrlServiceMock.Setup(x => x.GetAsync()).ReturnsAsync("https://api.nuget.org/v3/index.json");
            _cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny)).Returns(true);

            // Act
            await _suiteCommand.InstallSuiteAsync(null, false);

            // Assert
            _loggerMock.Verify(x => x.LogInformation(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Func<It.IsAnyType, Exception, string>>(), It.IsAny<Exception>()), Times.Exactly(2));
        }

        [Fact]
        public async Task InstallSuiteAsync_ShouldShowManualInstallCommand_WhenCmdHelperReturnsNonZeroExitCode()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs(new[] { "suite", "install" });
            _nuGetIndexUrlServiceMock.Setup(x => x.GetAsync()).ReturnsAsync("https://api.nuget.org/v3/index.json");
            _cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny)).Returns(false);

            // Act
            await _suiteCommand.InstallSuiteAsync(null, false);

            // Assert
            _loggerMock.Verify(x => x.LogInformation(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Func<It.IsAnyType, Exception, string>>(), It.IsAny<Exception>()), Times.Exactly(2));
            _loggerMock.Verify(x => x.LogError(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Func<It.IsAnyType, Exception, string>>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task InstallSuiteAsync_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs(new[] { "suite", "install" });
            _nuGetIndexUrlServiceMock.Setup(x => x.GetAsync()).ReturnsAsync("https://api.nuget.org/v3/index.json");
            _cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny)).Throws(new Exception("Test exception"));

            // Act
            await _suiteCommand.InstallSuiteAsync(null, false);

            // Assert
            _loggerMock.Verify(x => x.LogInformation(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Func<It.IsAnyType, Exception, string>>(), It.IsAny<Exception>()), Times.Exactly(2));
            _loggerMock.Verify(x => x.LogError(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Func<It.IsAnyType, Exception, string>>(), It.IsAny<Exception>()), Times.Once);
        }
    }
}
