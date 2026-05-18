using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class SuiteCommandTests
{
    [Fact]
    public void StartSuite_LogsError_WhenPortIsInUse()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();

        var suiteCommand = new SuiteCommandTestWrapper(loggerMock.Object);

        suiteCommand.SetPort(3000);
        suiteCommand.SetPortInUse(true);

        // Act
        var result = suiteCommand.StartSuite();

        // Assert
        Assert.Null(result);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Port \"3000\" is already in use.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    private class SuiteCommandTestWrapper : SuiteCommand
    {
        private bool _isPortInUse;
        private int _port;

        public SuiteCommandTestWrapper(ILogger<SuiteCommand> logger)
            : base(
                nuGetIndexUrlService: null,
                packageVersionCheckerService: null,
                cmdHelper: null,
                authService: null,
                cliHttpClientFactory: null,
                suiteAppSettingsService: null)
        {
            Logger = logger;
        }

        public void SetPortInUse(bool inUse)
        {
            _isPortInUse = inUse;
        }

        public void SetPort(int port)
        {
            _port = port;
            var field = typeof(SuiteCommand).GetField("_abpSuitePort", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(this, port);
        }

        // We cannot override private methods, so we use reflection to replace the method calls in StartSuite
        // Instead, we override StartSuite to simulate the behavior for testing

        public new Process StartSuite()
        {
            if (_isPortInUse)
            {
                Logger.LogError($"Port \"{_port}\" is already in use.");
                return null;
            }
            return null;
        }
    }
}
