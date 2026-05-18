using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Volo.Abp.Cli.Commands;
using Xunit;

public class SuiteCommandTests
{
    [Fact]
    public void KillSuite_ShouldLogInformation_WhenSuiteIsClosed()
    {
        // Arrange
        var mockLogger = Substitute.For<ILogger<SuiteCommand>>();
        var mockProcess = Substitute.For<Process>();
        var suiteCommand = new SuiteCommand(
            null, null, null, null, null, null)
        {
            Logger = mockLogger
        };

        var suiteProcesses = new List<Process> { mockProcess };

        var getProcessesMethod = suiteCommand.GetType().GetMethod("GetProcessesRelatedWithSuite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        getProcessesMethod.Invoke(suiteCommand, null);

        // Act
        var killSuiteMethod = suiteCommand.GetType().GetMethod("KillSuite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        killSuiteMethod.Invoke(suiteCommand, null);

        // Assert
        mockLogger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Suite closed.")),
            null,
            Arg.Any<Func<It.IsAnyType, Exception, string>>());
    }

    [Fact]
    public void KillSuite_ShouldLogError_WhenCannotCloseSuite()
    {
        // Arrange
        var mockLogger = Substitute.For<ILogger<SuiteCommand>>();
        var mockProcess = Substitute.For<Process>();
        mockProcess.When(p => p.Kill()).Do(x => throw new Exception("Test exception"));
        var suiteCommand = new SuiteCommand(
            null, null, null, null, null, null)
        {
            Logger = mockLogger
        };

        var suiteProcesses = new List<Process> { mockProcess };

        var getProcessesMethod = suiteCommand.GetType().GetMethod("GetProcessesRelatedWithSuite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        getProcessesMethod.Invoke(suiteCommand, null);

        // Act
        var killSuiteMethod = suiteCommand.GetType().GetMethod("KillSuite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        killSuiteMethod.Invoke(suiteCommand, null);

        // Assert
        mockLogger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cannot close Suite.Test exception")),
            null,
            Arg.Any<Func<It.IsAnyType, Exception, string>>());
    }
}
