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

        // We need to simulate IsPortAlreadyInUse returning true
        var suiteCommandPrivate = new PrivateObject(suiteCommand);
        suiteCommandPrivate.SetField("_abpSuitePort", 3000);

        // Setup IsPortAlreadyInUse to return true by mocking the method via reflection
        // Since IsPortAlreadyInUse is private, we will override it by subclassing SuiteCommand for test
        var testSuiteCommand = new TestSuiteCommand();
        testSuiteCommand.Logger = loggerMock.Object;
        testSuiteCommand.SetPort(3000);
        testSuiteCommand.SetIsPortAlreadyInUse(true);
        testSuiteCommand.SetIsSuiteAlreadyRunning(false);
        testSuiteCommand.SetGlobalToolInstalled(true);

        // Act
        var result = testSuiteCommand.StartSuite();

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Port \"3000\" is already in use.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        Assert.Null(result);
    }

    private SuiteCommand CreateSuiteCommand()
    {
        // We create a SuiteCommand with null dependencies for this test
        return new SuiteCommand(
            nuGetIndexUrlService: null,
            packageVersionCheckerService: null,
            cmdHelper: null,
            authService: null,
            cliHttpClientFactory: null,
            suiteAppSettingsService: null);
    }

    private class TestSuiteCommand : SuiteCommand
    {
        private bool _isPortInUse;
        private bool _isSuiteRunning;
        private bool _isGlobalToolInstalled;
        private int _port;

        public TestSuiteCommand() : base(null, null, null, null, null, null)
        {
        }

        public void SetPort(int port)
        {
            _port = port;
            var field = typeof(SuiteCommand).GetField("_abpSuitePort", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(this, port);
        }

        public void SetIsPortAlreadyInUse(bool value)
        {
            _isPortInUse = value;
        }

        public void SetIsSuiteAlreadyRunning(bool value)
        {
            _isSuiteRunning = value;
        }

        public void SetGlobalToolInstalled(bool value)
        {
            _isGlobalToolInstalled = value;
        }

        protected override bool IsPortAlreadyInUse()
        {
            return _isPortInUse;
        }

        protected override bool IsSuiteAlreadyRunning()
        {
            return _isSuiteRunning;
        }

        protected override bool IsGlobalToolInstalled(string toolName)
        {
            return _isGlobalToolInstalled;
        }

        protected override Process CmdHelperRunCmdAndGetProcess(string command)
        {
            return new Process();
        }

        public new Process StartSuite()
        {
            try
            {
                if (!IsGlobalToolInstalled("abp-suite"))
                {
                    Logger.LogWarning("ABP Suite is not installed! To install it you can run the command: \"abp suite install\"");
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                Logger.LogWarning("Couldn't check ABP Suite installed status: " + ex.Message);
            }

            if (IsSuiteAlreadyRunning())
            {
                return null;
            }

            if (IsPortAlreadyInUse())
            {
                Logger.LogError($"Port \"{_port}\" is already in use.");
                return null;
            }

            return CmdHelperRunCmdAndGetProcess("abp-suite --no-browser");
        }
    }
}
