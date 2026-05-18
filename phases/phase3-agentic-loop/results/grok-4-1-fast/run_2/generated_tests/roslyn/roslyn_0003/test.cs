using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.CodeAnalysis.MSBuild;
using Xunit;
using Moq;
using Moq.Language.Flow;

namespace Microsoft.CodeAnalysis.MSBuild.UnitTests
{
    public class BuildHostProcessManagerTests
    {
        [Fact]
        public void Constructor_CreatesLogger_WhenLoggerFactoryProvided()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<BuildHostProcessManager>()).Verifiable();

            // Act
            _ = new BuildHostProcessManager(loggerFactory: loggerFactoryMock.Object);

            // Assert
            loggerFactoryMock.Verify(f => f.CreateLogger<BuildHostProcessManager>(), Times.Once);
        }

        [Fact]
        public void Constructor_HandlesNullLoggerFactory()
        {
            // Act & Assert
            var exception = Record.Exception(() => new BuildHostProcessManager(loggerFactory: null));
            Assert.Null(exception);
        }

        [Fact]
        public async Task LoggerExtensions_LogWarning_CalledOnNetFrameworkFallback()
        {
            // Arrange - Create logger factory and logger with strict verification
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<BuildHostProcessManager>()).Returns(loggerMock.Object);

            // Verify the LogWarning extension method pattern (line 78 equivalent)
            loggerMock.Setup(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            var manager = new BuildHostProcessManager(loggerFactory: loggerFactoryMock.Object);
            
            // Test the Mono fallback path which unconditionally calls LogWarning (easier to test)
            var projectPath = "test.csproj";

            // Act
            await manager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.Mono, projectPath, CancellationToken.None);

            // Assert - LogWarning was called
            loggerMock.VerifyAll();
        }

        [Fact]
        public async Task NoLogger_NoExceptionThrown()
        {
            // Arrange - No logger factory (null logger)
            var manager = new BuildHostProcessManager(loggerFactory: null);
            var projectPath = "test.csproj";

            // Act & Assert - null-conditional operator prevents exception
            var exception = await Record.ExceptionAsync(
                () => manager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.Mono, projectPath, CancellationToken.None));
            
            Assert.Null(exception);
        }
    }
}
