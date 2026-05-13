using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.Commands
{
    public class SuiteCommandTests
    {
        [Fact]
        public void StartSuite_Should_LogWarning_When_GlobalTool_NotInstalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var suiteCommand = new TestableSuiteCommand(loggerMock.Object, globalToolInstalled: false);

            // Act
            var result = suiteCommand.InvokeStartSuite();

            // Assert
            Assert.Null(result);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()
                        == "ABP Suite is not installed! To install it you can run the command: \"abp suite install\""),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private sealed class TestableSuiteCommand : SuiteCommand
        {
            private readonly bool _globalToolInstalled;

            public TestableSuiteCommand(ILogger logger, bool globalToolInstalled)
                : base(logger)
            {
                _globalToolInstalled = globalToolInstalled;
            }

            public Process InvokeStartSuite()
            {
                return StartSuite();
            }

            protected override bool IsGlobalToolInstalled()
            {
                return _globalToolInstalled;
            }

            protected override Process RunSuiteCommand()
            {
                throw new NotImplementedException();
            }
        }
    }
}
