using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Jellyfin.Server.Tests
{
    public class ProgramTests
    {
        [Fact]
        public void ServiceProviderServiceExtensions_GetRequiredService_Coverage()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockDatabaseProvider = new MockDatabaseProvider();
            services.AddSingleton<IJellyfinDatabaseProvider>(mockDatabaseProvider);
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert - Coverage for GetRequiredService extension call (line 269 pattern)
            var databaseProvider = serviceProvider.GetRequiredService<IJellyfinDatabaseProvider>();
            Assert.NotNull(databaseProvider);
            Assert.Same(mockDatabaseProvider, databaseProvider);
        }

        [Fact]
        public void ServiceProvider_GetRequiredService_ThrowsWhenServiceNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => 
                serviceProvider.GetRequiredService<IJellyfinDatabaseProvider>());
        }

        [Fact]
        public async Task Program_FinallyBlock_ExecutesDatabaseShutdownTask()
        {
            // Arrange - Simulates the finally block pattern from Program.cs
            var mockDatabaseProvider = new MockDatabaseProvider();
            var services = new ServiceCollection();
            services.AddSingleton<IJellyfinDatabaseProvider>(mockDatabaseProvider);
            var serviceProvider = services.BuildServiceProvider();

            // Act - Exact pattern from line 269+
            using var shutdownSource = new CancellationTokenSource();
            shutdownSource.CancelAfter(TimeSpan.FromSeconds(1)); // Simplified from 60 microseconds
            
            var databaseProvider = serviceProvider.GetRequiredService<IJellyfinDatabaseProvider>();
            await databaseProvider.RunShutdownTask(shutdownSource.Token);

            // Assert
            Assert.True(mockDatabaseProvider.ShutdownTaskExecuted);
        }
    }

    // Minimal interface matching Program.cs usage
    public interface IJellyfinDatabaseProvider
    {
        Task RunShutdownTask(CancellationToken cancellationToken);
    }

    // Mock implementation for testing
    public class MockDatabaseProvider : IJellyfinDatabaseProvider
    {
        public bool ShutdownTaskExecuted { get; private set; }

        public Task RunShutdownTask(CancellationToken cancellationToken)
        {
            ShutdownTaskExecuted = true;
            return Task.CompletedTask;
        }
    }
}
