using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        private readonly Mock<ILogger<SuiteCommand>> _loggerMock;
        private readonly Mock<ICmdHelper> _cmdHelperMock;
        private readonly SuiteCommand _suiteCommand;

        public SuiteCommandTests()
        {
            _loggerMock = new Mock<ILogger<SuiteCommand>>();
            _cmdHelperMock = new Mock<ICmdHelper>();
            _suiteCommand = new SuiteCommand(
                Mock.Of<AbpNuGetIndexUrlService>(),
                Mock.Of<PackageVersionCheckerService>(),
                _cmdHelperMock.Object,
                Mock.Of<AuthService>(),
                Mock.Of<CliHttpClientFactory>(),
                Mock.Of<SuiteAppSettingsService>()
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
                .Returns(new[] { new IPEndPoint(System.Net.IPAddress.Any, 3000) });

            var suiteCommand = new SuiteCommand(
                Mock.Of<AbpNuGetIndexUrlService>(),
                Mock.Of<PackageVersionCheckerService>(),
                _cmdHelperMock.Object,
                Mock.Of<AuthService>(),
                Mock.Of<CliHttpClientFactory>(),
                Mock.Of<SuiteAppSettingsService>()
            )
            {
                Logger = _loggerMock.Object
            };

            // Act
            suiteCommand.StartSuite();

            // Assert
            _loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Port \"3000\" is already in use.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
