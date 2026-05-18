using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
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
                null, null, _cmdHelperMock.Object, _authServiceMock.Object, _cliHttpClientFactoryMock.Object, null)
            {
                Logger = _loggerMock.Object
            };

            // Mock GlobalToolHelper.IsGlobalToolInstalled to return false
            var globalToolHelperMock = new Mock<GlobalToolHelper>();
            globalToolHelperMock.Setup(g => g.IsGlobalToolInstalled("abp-suite")).Returns(false);

            // Act
            var result = suiteCommand.StartSuite();

            // Assert
            globalToolHelperMock.Verify(g => g.IsGlobalToolInstalled("abp-suite"), Times.Once);
            _loggerMock.Verify(
                x => x.LogWarning("ABP Suite is not installed! To install it you can run the command: \"abp suite install\""),
                Times.Once);
        }
    }
}
