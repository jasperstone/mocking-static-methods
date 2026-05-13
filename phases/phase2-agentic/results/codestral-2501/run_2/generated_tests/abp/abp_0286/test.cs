using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json.Linq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class SuiteCommandTests
    {
        private readonly Mock<ILogger<SuiteCommand>> _loggerMock;
        private readonly SuiteCommand _suiteCommand;

        public SuiteCommandTests()
        {
            _loggerMock = new Mock<ILogger<SuiteCommand>>();
            _suiteCommand = new SuiteCommand(
                new AbpNuGetIndexUrlService(),
                new PackageVersionCheckerService(),
                new CmdHelper(),
                new AuthService(),
                new CliHttpClientFactory(),
                new SuiteAppSettingsService()
            )
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public void StartSuite_WhenPortIsAlreadyInUse_LogsError()
        {
            // Arrange
            var ipGlobalPropertiesMock = new Mock<IPGlobalProperties>();
            ipGlobalPropertiesMock.Setup(x => x.GetActiveTcpListeners())
                .Returns(new[] { new IPEndPoint(0, 3000) });

            // Act
            var result = _suiteCommand.StartSuite();

            // Assert
            _loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void StartSuite_WhenSuiteIsNotInstalled_LogsWarning()
        {
            // Arrange
            var globalToolHelperMock = new Mock<GlobalToolHelper>();
            globalToolHelperMock.Setup(x => x.IsGlobalToolInstalled("abp-suite"))
                .Returns(false);

            // Act
            var result = _suiteCommand.StartSuite();

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void StartSuite_WhenSuiteIsAlreadyRunning_ReturnsNull()
        {
            // Arrange
            var processMock = new Mock<Process>();
            processMock.Setup(x => x.HasExited).Returns(false);

            // Act
            var result = _suiteCommand.StartSuite();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void StartSuite_WhenAllConditionsAreMet_RunsSuite()
        {
            // Arrange
            var cmdHelperMock = new Mock<ICmdHelper>();
            cmdHelperMock.Setup(x => x.RunCmdAndGetProcess("abp-suite --no-browser"))
                .Returns(new Process());

            // Act
            var result = _suiteCommand.StartSuite();

            // Assert
            Assert.NotNull(result);
        }
    }
}
