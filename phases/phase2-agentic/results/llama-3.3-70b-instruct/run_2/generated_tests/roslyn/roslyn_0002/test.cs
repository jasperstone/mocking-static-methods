using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.CodeAnalysis.MSBuild
{
    public class BuildHostProcessManagerTests
    {
        [Fact]
        public async Task GetBuildHostWithFallbackAsync_LogsWarningWhenMonoMSBuildIsNotInstalled()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<BuildHostProcessManager>();
            var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: loggerFactory);

            // Act
            await buildHostProcessManager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.Mono, "project.csproj", CancellationToken.None);

            // Assert
            loggerFactory.AssertLogged(logger, LogLevel.Warning, "An installation of Mono MSBuild could not be found; project.csproj will be loaded with the .NET Core SDK and may encounter errors.");
        }

        [Fact]
        public async Task GetBuildHostWithFallbackAsync_LogsWarningWhenNetFrameworkMSBuildIsNotInstalled()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<BuildHostProcessManager>();
            var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: loggerFactory);
            var buildHost = new Mock<RemoteBuildHost>();
            buildHost.Setup(b => b.HasUsableMSBuildAsync("project.csproj", CancellationToken.None)).ReturnsAsync(false);

            // Act
            await buildHostProcessManager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.NetFramework, "project.csproj", CancellationToken.None);

            // Assert
            loggerFactory.AssertLogged(logger, LogLevel.Warning, "An installation of Visual Studio or the Build Tools for Visual Studio could not be found; project.csproj will be loaded with the .NET Core SDK and may encounter errors.");
        }
    }

    public static class LoggerFactoryExtensions
    {
        public static void AssertLogged(this ILoggerFactory loggerFactory, ILogger logger, LogLevel logLevel, string message)
        {
            var loggerProvider = (LoggerProvider)loggerFactory;
            var loggerProviderMock = new Mock<LoggerProvider>();
            loggerProviderMock.Setup(lp => lp.CreateLogger(It.IsAny<string>())).Returns(logger);

            loggerFactory.AddProvider(loggerProviderMock.Object);

            var logged = false;
            loggerProviderMock.Setup(lp => lp.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception, string>>((level, id, state, ex, formatter) =>
                {
                    if (level == logLevel && state.ToString() == message)
                    {
                        logged = true;
                    }
                });

            Assert.True(logged);
        }
    }
}
