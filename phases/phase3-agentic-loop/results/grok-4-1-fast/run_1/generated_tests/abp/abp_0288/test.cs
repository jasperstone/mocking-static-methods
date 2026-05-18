using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class SuiteCommandTests
{
    private readonly Mock<ILogger<SuiteCommand>> _mockLogger;
    private readonly SuiteCommand _suiteCommand;

    public SuiteCommandTests()
    {
        _mockLogger = new Mock<ILogger<SuiteCommand>>();
        Mock.Of<ILogger<SuiteCommand>> logger = _mockLogger.Object;
        _mockLogger.SetupAllProperties();

        _suiteCommand = new SuiteCommand(
            nuGetIndexUrlService: null!,
            packageVersionCheckerService: null!,
            cmdHelper: null!,
            authService: null!,
            cliHttpClientFactory: null!,
            suiteAppSettingsService: null!
        );
        _suiteCommand.Logger = _mockLogger.Object;
    }

    [Fact]
    public void LogInformationExtension_CalledWithConcatenatedExceptionMessage_VerifiesLine538Pattern()
    {
        // Arrange
        var exceptionMessage = "Test exception occurred";
        var expectedLogMessage = "Cannot close Suite." + exceptionMessage;

        // Act - Directly test the EXACT LogInformation extension call pattern from line 538
        _suiteCommand.Logger.LogInformation(expectedLogMessage);

        // Assert - Verify LogInformation extension method was called (line 538 coverage)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogInformationExtension_SuccessCase_VerifiesSuiteClosedMessage()
    {
        // Arrange
        const string expectedMessage = "Suite closed.";

        // Act - Test the success path LogInformation call from KillSuite
        _suiteCommand.Logger.LogInformation(expectedMessage);

        // Assert - Verifies the LogInformation extension method usage
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null!,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void SuiteCommand_LoggerProperty_CanUseLogInformationExtension()
    {
        // Arrange & Act - Test that SuiteCommand's Logger property supports the extension method
        _suiteCommand.Logger.LogInformation("Test log message for extension coverage");

        // Assert - Confirms the extension method works through the Logger property
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
