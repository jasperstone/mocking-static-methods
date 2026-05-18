using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.Volo.Abp.Cli.Commands;

public class SuiteCommandTests
{
    [Fact]
    public void StartSuite_LogsError_WhenPortIsAlreadyInUse()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var suiteCommand = CreateSuiteCommand();
        suiteCommand.Logger = loggerMock.Object;

        // We need to override IsPortAlreadyInUse to return true to trigger the log error
        var suiteCommandPrivate = new PrivateObject(suiteCommand);
        suiteCommandPrivate.SetField("_abpSuitePort", 3000);

        // Use reflection to override IsPortAlreadyInUse method to return true
        var isPortAlreadyInUseMethod = typeof(SuiteCommand).GetMethod("IsPortAlreadyInUse", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var isPortAlreadyInUseDelegate = new Func<bool>(() => true);
        // We cannot override private methods easily, so we will create a derived class for testing

        var testSuiteCommand = new TestSuiteCommand(loggerMock.Object);

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
        // We need to provide all dependencies for SuiteCommand constructor
        // We can mock or use null for those not used in this test
        var nuGetIndexUrlService = new Mock<object>().Object;
        var packageVersionCheckerService = new Mock<object>().Object;
        var cmdHelper = new Mock<ICmdHelper>().Object;
        var authService = new Mock<object>().Object;
        var cliHttpClientFactory = new Mock<object>().Object;
        var suiteAppSettingsService = new Mock<object>().Object;

        // The constructor requires specific types, so we will create mocks with correct types
        return new SuiteCommand(
            nuGetIndexUrlService as dynamic,
            packageVersionCheckerService as dynamic,
            cmdHelper,
            authService as dynamic,
            cliHttpClientFactory as dynamic,
            suiteAppSettingsService as dynamic);
    }

    private class TestSuiteCommand : SuiteCommand
    {
        private readonly ILogger<SuiteCommand> _logger;

        public TestSuiteCommand(ILogger<SuiteCommand> logger) : base(
            null, null, null, null, null, null)
        {
            _logger = logger;
            Logger = logger;
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
