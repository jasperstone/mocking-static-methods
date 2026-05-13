using Moq;
using System.Net.NetworkInformation;
using Xunit;
using Volo.Abp.Cli.Commands;

public class SuiteCommandTests
{
    private readonly Mock<ILogger<SuiteCommand>> _loggerMock;
    private readonly SuiteCommand _suiteCommand;

    public SuiteCommandTests()
    {
        _loggerMock = new Mock<ILogger<SuiteCommand>>();
        _suiteCommand = new SuiteCommand(
            null, // AbpNuGetIndexUrlService
            null, // PackageVersionCheckerService
            null, // ICmdHelper
            null, // AuthService
            null, // CliHttpClientFactory
            null  // SuiteAppSettingsService
        )
        {
            Logger = _loggerMock.Object
        };
    }

    [Fact]
    public void StartSuite_PortAlreadyInUse_LogsError()
    {
        // Arrange
        var ipGlobalPropertiesMock = new Mock<IPGlobalProperties>();
        var tcpListenersMock = new Mock<ReadOnlyCollection<IPGlobalProperties.TcpConnectionInformation>>();
        tcpListenersMock.Setup(m => m.GetEnumerator()).Returns(new List<IPGlobalProperties.TcpConnectionInformation>
        {
            new IPGlobalProperties.TcpConnectionInformation(new IPEndPoint(IPAddress.Loopback, _suiteCommand._abpSuitePort), null, null, null, null)
        }.GetEnumerator());

        ipGlobalPropertiesMock.Setup(m => m.GetActiveTcpListeners()).Returns(tcpListenersMock.Object);

        System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties = () => ipGlobalPropertiesMock.Object;

        // Act
        var process = _suiteCommand.StartSuite();

        // Assert
        _loggerMock.Verify(
            logger => logger.LogError(It.Is<string>(s => s.Contains($"Port \"{_suiteCommand._abpSuitePort}\" is already in use."))),
            Times.Once
        );

        Assert.Null(process);
    }
}
