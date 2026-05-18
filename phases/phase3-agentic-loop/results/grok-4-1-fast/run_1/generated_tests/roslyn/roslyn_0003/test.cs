using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild.UnitTests;

public class BuildHostProcessManagerTests
{
    private sealed class TestableManager : BuildHostProcessManager
    {
        public RemoteBuildHost? FirstBuildHostResult { get; set; }
        public RemoteBuildHost? SecondBuildHostResult { get; set; }
        public bool HasUsableMSBuildResult { get; set; } = true;

        public TestableManager(ILoggerFactory? loggerFactory = null) : base(loggerFactory: loggerFactory)
        {
        }

        public override async Task<RemoteBuildHost> GetBuildHostAsync(BuildHostProcessKind buildHostKind, string? projectOrSolutionFilePath, string? dotnetPath, CancellationToken cancellationToken)
        {
            if (FirstBuildHostResult is not null)
            {
                var result = FirstBuildHostResult;
                FirstBuildHostResult = null;
                return result;
            }

            if (SecondBuildHostResult is not null)
            {
                return SecondBuildHostResult;
            }

            throw new InvalidOperationException("Not configured");
        }
    }

    private sealed class MockLogger : ILogger
    {
        public IReadOnlyList<string> Warnings => _warnings;
        private readonly List<string> _warnings = new();

        public IDisposable? BeginScope<TState>(TState state) => null!;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Warning)
            {
                _warnings.Add(formatter(state, exception));
            }
        }
    }

    [Fact]
    public async Task LogsWarning_WhenMonoMSBuildNotFound()
    {
        // Arrange - make MonoMSBuildDiscovery return null by clearing cache if possible
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var mockLogger = new MockLogger();
        loggerFactoryMock.Setup(f => f.CreateLogger<BuildHostProcessManager>()).Returns(mockLogger);

        var manager = new TestableManager(loggerFactoryMock.Object);

        // Act
        var (_, actualKind) = await manager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.Mono, "test.csproj", CancellationToken.None);

        // Assert - warning logged and fallback occurred
        Assert.Contains(mockLogger.Warnings, w => w.Contains("Mono MSBuild") && w.Contains("test.csproj"));
        Assert.Equal(BuildHostProcessKind.NetCore, actualKind);
    }

    [Fact]
    public async Task LogsWarning_WhenNetFrameworkBuildHostNotUsable()
    {
        // Arrange
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var mockLogger = new MockLogger();
        loggerFactoryMock.Setup(f => f.CreateLogger<BuildHostProcessManager>()).Returns(mockLogger);

        var mockBuildHost = new Mock<RemoteBuildHost>();
        mockBuildHost.Setup(h => h.HasUsableMSBuildAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(false);

        var manager = new TestableManager(loggerFactoryMock.Object)
        {
            FirstBuildHostResult = mockBuildHost.Object,
            SecondBuildHostResult = mockBuildHost.Object,
            HasUsableMSBuildResult = false
        };

        // Act
        var (_, actualKind) = await manager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.NetFramework, "test.csproj", CancellationToken.None);

        // Assert
        Assert.Contains(mockLogger.Warnings, w => w.Contains("Visual Studio") && w.Contains("test.csproj"));
        Assert.Equal(BuildHostProcessKind.NetCore, actualKind);
    }

    [Fact]
    public async Task DoesNotLogWarning_WhenNoLogger()
    {
        // Arrange
        var manager = new TestableManager(loggerFactory: null);

        // Act
        var result = await manager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.Mono, "test.csproj", CancellationToken.None);

        // Assert - no exception
        Assert.NotNull(result);
    }
}
