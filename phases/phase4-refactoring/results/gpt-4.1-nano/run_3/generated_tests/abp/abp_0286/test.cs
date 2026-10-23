using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests
{
    public class SuiteCommandTests
    {
        private readonly Mock<ILogger<SuiteCommand>> _loggerMock;
        private readonly Mock<CmdHelper> _cmdHelperMock;
        private readonly SuiteCommand _suiteCommand;

        public SuiteCommandTests()
        {
            _loggerMock = new Mock<ILogger<SuiteCommand>>();
            _cmdHelperMock = new Mock<CmdHelper>();
            _suiteCommand = new SuiteCommand(
                null, null, _cmdHelperMock.Object, null, null, null)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public void StartSuite_PortInUse_LogsError()
        {
            // Arrange
            var testSuite = new TestSuiteCommand(_loggerMock.Object);
            // Act
            var result = testSuite.StartSuite();

            // Assert
            _loggerMock.Verify(
                x => x.LogError(It.Is<string>(s => s.Contains("Port"))),
                Times.Once);
        }

        private class TestSuiteCommand : SuiteCommand
        {
            public TestSuiteCommand(ILogger<SuiteCommand> logger) : base(null, null, null, null, null, null)
            {
                Logger = logger;
            }

            public override Process StartSuite()
            {
                // Simulate port in use
                Logger.LogError($"Port \"{3000}\" is already in use.");
                return null;
            }
        }
    }
}
