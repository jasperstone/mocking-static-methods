using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests
{
    public class SuiteCommandTests
    {
        private readonly Mock<ILogger<SuiteCommand>> _loggerMock;
        private readonly Mock<ICmdHelper> _cmdHelperMock;
        private readonly Mock<SuiteAppSettingsService> _suiteAppSettingsServiceMock;
        private readonly Mock<AuthService> _authServiceMock;
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;

        public SuiteCommandTests()
        {
            _loggerMock = new Mock<ILogger<SuiteCommand>>();
            _cmdHelperMock = new Mock<ICmdHelper>();
            _suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();
            _authServiceMock = new Mock<AuthService>();
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        }

        [Fact]
        public void StartSuite_ShouldLogWarning_WhenGlobalToolNotInstalled()
        {
            // Arrange
            var suiteCommand = new SuiteCommand(
                new Mock<AbpNuGetIndexUrlService>().Object,
                new Mock<PackageVersionCheckerService>().Object,
                _cmdHelperMock.Object,
                _authServiceMock.Object,
                _cliHttpClientFactoryMock.Object,
                new Mock<SuiteAppSettingsService>().Object
            )
            {
                Logger = _loggerMock.Object
            };

            // Mock static method IsGlobalToolInstalled to return false
            var mockHelper = new Mock<SuiteCommand>();
            mockHelper.Setup(s => s.IsGlobalToolInstalled("abp-suite")).Returns(false);

            // Act
            var result = suiteCommand.StartSuite();

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning("ABP Suite is not installed! To install it you can run the command: \"abp suite install\""),
                Times.Once);
        }
    }
}
