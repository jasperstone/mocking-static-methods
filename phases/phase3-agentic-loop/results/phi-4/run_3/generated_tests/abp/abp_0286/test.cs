using Moq;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.NetworkInformation;
using Xunit;
using Volo.Abp.Cli.Commands;
using Microsoft.Extensions.Logging;

public class SuiteCommandTests
{
    [Fact]
    public void StartSuite_LogsError_WhenPortIsAlreadyInUse()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var ipGlobalPropertiesMock = new Mock<IPGlobalProperties>();
        var tcpListenersMock = new ReadOnlyCollection<IPAddressInformation>(
            new List<IPAddressInformation>
            {
                new IPEndPoint(IPAddress.Loopback, 3000).MapToIPv4().ToIPAddressInformation()
            });

        ipGlobalPropertiesMock.Setup(ip => ip.GetActiveTcpListeners()).Returns(new[] { tcpListenersMock });

        var suiteCommand = new SuiteCommand(
            null, // AbpNuGetIndexUrlService
            null, // PackageVersionCheckerService
            null, // ICmdHelper
            null, // AuthService
            null, // CliHttpClientFactory
            null  // SuiteAppSettingsService
        )
        {
            Logger = loggerMock.Object,
            _abpSuitePort = 3000 // Directly setting the private field for testing
        };

        // Act
        suiteCommand.StartSuite();

        // Assert
        loggerMock.Verify(
            logger => logger.LogError(It.Is<string>(s => s.Contains("Port \"3000\" is already in use."))),
            Times.Once);
    }
}
