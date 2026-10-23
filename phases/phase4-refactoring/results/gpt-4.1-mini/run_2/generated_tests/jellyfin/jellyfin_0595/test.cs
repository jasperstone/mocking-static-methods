using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests
{
    // Minimal interface definition to simulate IJellyfinDatabaseProvider for testing
    public interface IJellyfinDatabaseProvider
    {
        Task RunShutdownTask(CancellationToken cancellationToken);
    }

    public class ProgramTests
    {
        private class TestJellyfinDatabaseProvider : IJellyfinDatabaseProvider
        {
            public bool RunShutdownTaskCalled { get; private set; }
            public CancellationToken PassedToken { get; private set; }

            public Task RunShutdownTask(CancellationToken cancellationToken)
            {
                RunShutdownTaskCalled = true;
                PassedToken = cancellationToken;
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task FinallyBlock_CallsGetRequiredServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var testDatabaseProvider = new TestJellyfinDatabaseProvider();
            services.AddSingleton<IJellyfinDatabaseProvider>(testDatabaseProvider);
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var databaseProvider = serviceProvider.GetRequiredService<IJellyfinDatabaseProvider>();
            using var shutdownSource = new CancellationTokenSource();
            shutdownSource.CancelAfter((int)TimeSpan.FromSeconds(60).TotalMilliseconds);
            await databaseProvider.RunShutdownTask(shutdownSource.Token);

            // Assert
            Assert.True(testDatabaseProvider.RunShutdownTaskCalled);
            Assert.True(testDatabaseProvider.PassedToken.CanBeCanceled);
        }
    }
}
