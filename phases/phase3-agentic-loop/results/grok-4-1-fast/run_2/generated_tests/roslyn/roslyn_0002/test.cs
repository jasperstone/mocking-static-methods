using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.CodeAnalysis.MSBuild;

namespace Microsoft.CodeAnalysis.MSBuild.UnitTests;

public class BuildHostProcessManagerTests
{
    [Fact]
    public void Constructor_CreatesLoggerCorrectly()
    {
        // Arrange
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger<BuildHostProcessManager>()).Returns<ILogger>(null!);

        // Act
        _ = new BuildHostProcessManager(loggerFactory: loggerFactoryMock.Object);

        // Assert
        loggerFactoryMock.Verify(f => f.CreateLogger<BuildHostProcessManager>(), Times.Once);
    }

    [Fact]
    public void Constructor_HandlesNullLoggerFactory()
    {
        // Act & Assert
        _ = new BuildHostProcessManager(loggerFactory: null);
    }
}
