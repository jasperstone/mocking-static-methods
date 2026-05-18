using System;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;

namespace Volo.Abp.Cli.Tests.Commands
{
    // Partial class to mock GlobalToolHelper
    public static class GlobalToolHelperMockable
    {
        public static Func<string, bool> IsGlobalToolInstalledFunc { get; set; } = GlobalToolHelper.IsGlobalToolInstalled;

        public static bool IsGlobalToolInstalled(string toolCommandName)
        {
            return IsGlobalToolInstalledFunc(toolCommandName);
        }
    }

    public class SuiteCommandTests
    {
        private readonly Mock<ILogger<SuiteCommand>> _loggerMock;
        private readonly SuiteCommand _suiteCommand;

        public SuiteCommandTests()
        {
            _loggerMock = new Mock<ILogger<SuiteCommand>>();

            var cmdHelperMock = new Mock<ICmdHelper>();
            var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();
            var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
            var authServiceMock = new Mock<AuthService>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();

            _suiteCommand = new SuiteCommand(
                nuGetIndexUrlServiceMock.Object,
                packageVersionCheckerServiceMock.Object,
                cmdHelperMock.Object,
                authServiceMock.Object,
                cliHttpClientFactoryMock.Object,
                suiteAppSettingsServiceMock.Object)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public void StartSuite_LogsWarning_WhenSuiteIsNotInstalled()
        {
            // Arrange
            GlobalToolHelperMockable.IsGlobalToolInstalledFunc = name => false;

            // Act
            var process = _suiteCommand.StartSuite();

            // Assert
            _loggerMock.Verify(
                logger => logger.LogWarning(
                    It.Is<string>(s => s.Contains("ABP Suite is not installed! To install it you can run the command: \"abp suite install\"")),
                    It.IsAny<Exception>()
                ),
                Times.Once
            );

            Assert.Null(process);
        }

        [Fact]
        public void StartSuite_DoesNotLogWarning_WhenSuiteIsInstalled()
        {
            // Arrange
            GlobalToolHelperMockable.IsGlobalToolInstalledFunc = name => true;

            // Act
            var process = _suiteCommand.StartSuite();

            // Assert
            _loggerMock.Verify(
                logger => logger.LogWarning(
                    It.IsAny<string>(),
                    It.IsAny<Exception>()
                ),
                Times.Never
            );

            Assert.NotNull(process);
        }
    }
}
