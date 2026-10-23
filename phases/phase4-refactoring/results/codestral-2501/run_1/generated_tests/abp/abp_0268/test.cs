using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public async Task InstallSuiteAsync_ShouldLogInformation_WhenPreviewIsTrue()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SuiteCommand>>();
            var mockNuGetIndexUrlService = new Mock<AbpNuGetIndexUrlService>();
            var suiteCommand = new SuiteCommand(
                mockNuGetIndexUrlService.Object,
                null,
                null,
                null,
                null,
                null
            )
            {
                Logger = mockLogger.Object
            };

            // Act
            await suiteCommand.InstallSuiteAsync(null, true);

            // Assert
            mockLogger.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("Latest preview version is")),
                    It.IsAny<object[]>()
                ),
                Times.Once
            );
        }
    }
}
