using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.NodeServices.Npm.Tests
{
    public class NodeScriptRunnerTests
    {
        [Fact]
        public void AttachToLogger_LogsError_WhenStdErrReceivesLine()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var nodeScriptRunner = new NodeScriptRunner(
                workingDirectory: "test",
                scriptName: "test-script",
                arguments: null,
                envVars: null,
                pkgManagerCommand: "npm",
                diagnosticSource: null,
                applicationStoppingToken: System.Threading.CancellationToken.None);

            // Act
            nodeScriptRunner.AttachToLogger(loggerMock.Object);
            nodeScriptRunner.StdErr.OnReceivedLine("Error message");

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error message")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
