using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;

namespace Volo.Abp.Cli.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public async Task StartSuite_Should_Log_Warning_When_GlobalTool_Not_Installed()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SuiteCommand>>();
            var mockCmdHelper = new Mock<ICmdHelper>();
            var mockAuthService = new Mock<AuthService>();
            var mockHttpClientFactory = new Mock<CliHttpClientFactory>();
            var mockSuiteAppSettingsService = new Mock<SuiteAppSettingsService>();
            var command = new SuiteCommand(
                new Mock<AbpNuGetIndexUrlService>().Object,
                new Mock<PackageVersionCheckerService>().Object,
                mockCmdHelper.Object,
                mockAuthService.Object,
                mockHttpClientFactory.Object,
                mockSuiteAppSettingsService.Object
            )
            {
                Logger = mockLogger.Object
            };

            // Use reflection to invoke the private method StartSuite
            var methodInfo = typeof(SuiteCommand).GetMethod("StartSuite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(methodInfo);

            // Act
            var result = methodInfo.Invoke(command, null);

            // Assert
            mockLogger.Verify(
                x => x.LogWarning("ABP Suite is not installed! To install it you can run the command: \"abp suite install\""),
                Times.Once);
        }
    }
}
