using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.Volo.Abp.Cli.Commands;

public class SuiteCommandTests
{
    [Fact]
    public void StartSuite_LogsError_WhenPortIsInUse()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SuiteCommand>>();

        var suiteCommand = new TestSuiteCommand();
        suiteCommand.Logger = mockLogger.Object;

        // Act
        var result = suiteCommand.StartSuite();

        // Assert
        Assert.Null(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Port \"3000\" is already in use.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    private class TestSuiteCommand : SuiteCommand
    {
        public TestSuiteCommand()
            : base(
                nuGetIndexUrlService: null,
                packageVersionCheckerService: null,
                cmdHelper: null,
                authService: null,
                cliHttpClientFactory: null,
                suiteAppSettingsService: null)
        {
            Logger = NullLogger<SuiteCommand>.Instance;
        }

        protected override bool IsPortAlreadyInUse()
        {
            return true;
        }

        protected override bool IsSuiteAlreadyRunning()
        {
            return false;
        }

        protected override bool GlobalToolHelper_IsGlobalToolInstalled(string toolName)
        {
            return true;
        }

        protected override Process CmdHelper_RunCmdAndGetProcess(string command)
        {
            return null;
        }
    }
}
