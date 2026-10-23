using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.Volo.Abp.Cli.Commands;

public class SuiteCommandTests
{
    [Fact]
    public void KillSuite_LogsInformationWhenCalledViaExecuteAsyncGenerate()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SuiteCommand>>();
        var suiteCommand = CreateSuiteCommand();
        suiteCommand.Logger = mockLogger.Object;

        // Act
        // Setup command line args with target "generate"
        var args = new CommandLineArgs(null, "generate");

        // We run ExecuteAsync and catch exceptions (since dependencies are null)
        var ex = Record.ExceptionAsync(() => suiteCommand.ExecuteAsync(args)).GetAwaiter().GetResult();

        // Assert
        // We expect no exception thrown from ExecuteAsync
        Assert.Null(ex);

        // We expect that Logger.LogInformation was called at least once with "Suite closed." or "Cannot close Suite."
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Suite closed.") || v.ToString().StartsWith("Cannot close Suite.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    private SuiteCommand CreateSuiteCommand()
    {
        // We create SuiteCommand with null dependencies for simplicity as they are not used in KillSuite
        return (SuiteCommand)Activator.CreateInstance(
            typeof(SuiteCommand),
            new object[] { null, null, null, null, null, null })!;
    }
}
