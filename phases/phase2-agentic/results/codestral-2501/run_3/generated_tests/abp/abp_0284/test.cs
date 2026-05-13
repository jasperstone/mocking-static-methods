using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        private readonly Mock<ILogger<SuiteCommand>> _loggerMock;
        private readonly Mock<ICmdHelper> _cmdHelperMock;
        private readonly Mock<AbpNuGetIndexUrlService> _nuGetIndexUrlServiceMock;
        private readonly Mock<PackageVersionCheckerService> _packageVersionCheckerServiceMock;
        private readonly Mock<AuthService> _authServiceMock;
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<SuiteAppSettingsService> _suiteAppSettingsServiceMock;
        private readonly SuiteCommand _suiteCommand;

        public SuiteCommandTests()
        {
            _loggerMock = new Mock<ILogger<SuiteCommand>>();
            _cmdHelperMock = new Mock<ICmdHelper>();
            _nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();
            _packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
            _authServiceMock = new Mock<AuthService>();
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            _suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();

            _suiteCommand = new SuiteCommand(
                _nuGetIndexUrlServiceMock.Object,
                _packageVersionCheckerServiceMock.Object,
                _cmdHelperMock.Object,
                _authServiceMock.Object,
                _cliHttpClientFactoryMock.Object,
                _suiteAppSettingsServiceMock.Object
            )
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public void StartSuite_WhenAbpSuiteIsNotInstalled_LogsWarning()
        {
            // Arrange
            _cmdHelperMock.Setup(x => x.RunCmdAndGetProcess(It.IsAny<string>())).Returns((Process)null);
            _cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), It.IsAny<string>()));
            _cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny, It.IsAny<string>()));
            _cmdHelperMock.Setup(x => x.RunCmdAndGetOutput(It.IsAny<string>(), It.IsAny<string>())).Returns(string.Empty);
            _cmdHelperMock.Setup(x => x.RunCmdAndGetOutput(It.IsAny<string>(), out It.Ref<int>.IsAny, It.IsAny<string>())).Returns(string.Empty);
            _cmdHelperMock.Setup(x => x.RunCmdAndGetOutput(It.IsAny<string>(), out It.Ref<bool>.IsAny, It.IsAny<string>())).Returns(string.Empty);
            _cmdHelperMock.Setup(x => x.RunCmdAndExit(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()));
            _cmdHelperMock.Setup(x => x.GetArguments(It.IsAny<string>(), It.IsAny<int?>())).Returns(string.Empty);
            _cmdHelperMock.Setup(x => x.GetFileName()).Returns(string.Empty);
            _cmdHelperMock.Setup(x => x.Open(It.IsAny<string>()));

            // Act
            var result = _suiteCommand.StartSuite();

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning("ABP Suite is not installed! To install it you can run the command: \"abp suite install\""),
                Times.Once
            );
            Assert.Null(result);
        }

        [Fact]
        public void StartSuite_WhenAbpSuiteIsInstalled_LogsNothing()
        {
            // Arrange
            _cmdHelperMock.Setup(x => x.RunCmdAndGetProcess(It.IsAny<string>())).Returns((Process)null);
            _cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), It.IsAny<string>()));
            _cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny, It.IsAny<string>()));
            _cmdHelperMock.Setup(x => x.RunCmdAndGetOutput(It.IsAny<string>(), It.IsAny<string>())).Returns(string.Empty);
            _cmdHelperMock.Setup(x => x.RunCmdAndGetOutput(It.IsAny<string>(), out It.Ref<int>.IsAny, It.IsAny<string>())).Returns(string.Empty);
            _cmdHelperMock.Setup(x => x.RunCmdAndGetOutput(It.IsAny<string>(), out It.Ref<bool>.IsAny, It.IsAny<string>())).Returns(string.Empty);
            _cmdHelperMock.Setup(x => x.RunCmdAndExit(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()));
            _cmdHelperMock.Setup(x => x.GetArguments(It.IsAny<string>(), It.IsAny<int?>())).Returns(string.Empty);
            _cmdHelperMock.Setup(x => x.GetFileName()).Returns(string.Empty);
            _cmdHelperMock.Setup(x => x.Open(It.IsAny<string>()));

            // Act
            var result = _suiteCommand.StartSuite();

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(It.IsAny<string>()),
                Times.Never
            );
            Assert.Null(result);
        }
    }
}
