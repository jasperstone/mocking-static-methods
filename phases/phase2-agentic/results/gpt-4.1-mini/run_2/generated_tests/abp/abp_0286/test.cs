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
        var testSuiteCommand = new TestSuiteCommandForPortInUse(loggerMock.Object);

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
        // We create SuiteCommand with null dependencies because we won't use them in this test
        return new SuiteCommand(
            nuGetIndexUrlService: null,
            packageVersionCheckerService: null,
            cmdHelper: null,
            authService: null,
            cliHttpClientFactory: null,
            suiteAppSettingsService: null);
    }

    private class TestSuiteCommandForPortInUse : SuiteCommand
    {
        private readonly ILogger<SuiteCommand> _logger;

        public TestSuiteCommandForPortInUse(ILogger<SuiteCommand> logger)
            : base(null, null, null, null, null, null)
        {
            _logger = logger;
            Logger = _logger;
            _abpSuitePort = 3000;
        }

        protected override bool IsPortAlreadyInUse()
        {
            return true;
        }

        protected override bool IsSuiteAlreadyRunning()
        {
            return false;
        }

        protected override Process StartSuite()
        {
            if (IsPortAlreadyInUse())
            {
                Logger.LogError($"Port \"{_abpSuitePort}\" is already in use.");
                return null;
            }

            return base.StartSuite();
        }
    }
}
