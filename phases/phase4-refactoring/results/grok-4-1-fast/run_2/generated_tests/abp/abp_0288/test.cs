using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Volo.Abp.Cli.Args;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class SuiteCommandTests
{
    [Fact]
    public void KillSuite_WhenExceptionOccurs_ShouldLogInformationWithExceptionMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SuiteCommand>>();
        mockLogger.SetupAllProperties();

        // Create SuiteCommand with mocked logger
        var cmdHelperMock = new Mock<ICmdHelper>();
        var suiteCommand = new SuiteCommandTestHelper(
            mockLogger.Object,
            cmdHelperMock.Object
        );

        // Force exception in GetProcessesRelatedWithSuite to hit line 538 catch block
        suiteCommand.ForceExceptionInProcesses = true;

        // Act
        suiteCommand.KillSuite();

        // Assert - Verify exact LogInformation call from line 538
        mockLogger.Verify(
            x => x.LogInformation("Cannot close Suite.Test process exception"),
            Times.Once
        );
    }

    [Fact]
    public void KillSuite_WhenNoException_ShouldNotLogExceptionMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SuiteCommand>>();
        mockLogger.SetupAllProperties();

        var cmdHelperMock = new Mock<ICmdHelper>();
        var suiteCommand = new SuiteCommandTestHelper(
            mockLogger.Object,
            cmdHelperMock.Object
        );

        // No exception forced - normal path

        // Act
        suiteCommand.KillSuite();

        // Assert - Verify line 538 catch block was NOT executed
        mockLogger.Verify(
            x => x.LogInformation(It.Is<string>(s => s.StartsWith("Cannot close Suite."))),
            Times.Never
        );
    }
}

// Test helper class that inherits SuiteCommand and makes KillSuite protected virtual for testing
public class SuiteCommandTestHelper : SuiteCommand
{
    public bool ForceExceptionInProcesses { get; set; }

    public SuiteCommandTestHelper(
        ILogger<SuiteCommand> logger,
        ICmdHelper cmdHelper) : base(
            new DummyNugetService(),
            new DummyPackageService(),
            cmdHelper,
            new DummyAuthService(),
            new DummyHttpFactory(),
            new DummySuiteSettings())
    {
        Logger = logger;
    }

    public new void KillSuite()
    {
        base.KillSuite();
    }

    private new IEnumerable<Process> GetProcessesRelatedWithSuite()
    {
        if (ForceExceptionInProcesses)
        {
            throw new InvalidOperationException("Test process exception");
        }
        return Array.Empty<Process>();
    }
}

// Minimal dummy implementations for constructor dependencies
public class DummyNugetService : AbpNuGetIndexUrlService
{
    public DummyNugetService() : base(NullLogger<AbpNuGetIndexUrlService>.Instance) { }
}

public class DummyPackageService : PackageVersionCheckerService
{
    public DummyPackageService() : base(NullLogger<PackageVersionCheckerService>.Instance) { }
}

public class DummyAuthService : AuthService
{
    public DummyAuthService() : base(NullLogger<AuthService>.Instance) { }
}

public class DummyHttpFactory : CliHttpClientFactory { }

public class DummySuiteSettings : SuiteAppSettingsService
{
    public DummySuiteSettings() : base(NullLogger<SuiteAppSettingsService>.Instance) { }
}
