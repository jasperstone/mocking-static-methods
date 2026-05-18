using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.CodeAnalysis.MSBuild;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.MSBuild.UnitTests
{
    public class BuildHostProcessManagerTests
    {
        [Fact]
        public async Task GetBuildHostWithFallbackAsync_MonoFallback_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
            loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<BuildHostProcessManager>()).Returns(loggerMock.Object);
            
            // Mock MonoMSBuildDiscovery to return null (triggers warning)
            var originalMethod = typeof(MonoMSBuildDiscovery).GetMethod("GetMonoMSBuildVersion", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
            
            var manager = new BuildHostProcessManager(loggerFactory: loggerFactoryMock.Object);

            // Act
            var (_, actualKind) = await manager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.Mono, "test.csproj", CancellationToken.None);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Mono MSBuild") && v.ToString()!.Contains("test.csproj")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
