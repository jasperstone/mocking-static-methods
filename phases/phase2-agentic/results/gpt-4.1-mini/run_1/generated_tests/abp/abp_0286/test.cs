using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class SuiteCommandTests
{
    [Fact]
    public void StartSuite_LogsError_WhenPortIsAlreadyInUse()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var suiteCommand = CreateSuiteCommand();
        suiteCommand.Logger = loggerMock.Object;

        // We need to override IsPortAlreadyInUse to return true to trigger the LogError call
        var suiteCommandMock = new SuiteCommandMock(suiteCommand);
        suiteCommandMock.SetPortInUse(true);

        // Act
        var result = suiteCommandMock.StartSuite();

        // Assert
        Assert.Null(result);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Port \"3000\" is already in use.")),
                It.IsAny<System.Exception>(),
                It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
            Times.Once);
    }

    private SuiteCommand CreateSuiteCommand()
    {
        // We create SuiteCommand with null dependencies because we only test StartSuite and override methods
        return new SuiteCommand(
            nuGetIndexUrlService: null,
            packageVersionCheckerService: null,
            cmdHelper: null,
            authService: null,
            cliHttpClientFactory: null,
            suiteAppSettingsService: null);
    }

    private class SuiteCommandMock : SuiteCommand
    {
        private bool _portInUse;

        public SuiteCommandMock(SuiteCommand original) : base(
            nuGetIndexUrlService: null,
            packageVersionCheckerService: null,
            cmdHelper: null,
            authService: null,
            cliHttpClientFactory: null,
            suiteAppSettingsService: null)
        {
            Logger = original.Logger;
            _abpSuitePort = 3000;
        }

        public void SetPortInUse(bool inUse)
        {
            _portInUse = inUse;
        }

        protected override bool IsPortAlreadyInUse()
        {
            return _portInUse;
        }

        protected override bool IsSuiteAlreadyRunning()
        {
            return false;
        }

        protected override Process CmdHelper_RunCmdAndGetProcess(string command)
        {
            return null;
        }

        public new Process StartSuite()
        {
            return base.StartSuite();
        }
    }
}
