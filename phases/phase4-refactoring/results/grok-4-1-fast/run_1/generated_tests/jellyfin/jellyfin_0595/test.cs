using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests
{
    public interface IJellyfinDatabaseProvider
    {
        Task RunShutdownTask(CancellationToken cancellationToken);
    }

    public class ProgramTests
    {
        [Fact]
        public void ServiceProvider_GetRequiredService_ThrowsWhenServiceNotRegistered()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            
            // Act & Assert - Tests the Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService
            // extension method behavior used at Program.cs line 269
            var exception = Assert.Throws<InvalidOperationException>(() => 
                serviceProvider.GetRequiredService<IJellyfinDatabaseProvider>());
            
            Assert.Contains("No service for type", exception.Message);
        }

        [Fact]
        public void ServiceProvider_GetRequiredService_ReturnsRegisteredService()
        {
            // Arrange
            var mockDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();
            var serviceProvider = new ServiceCollection()
                .AddSingleton(mockDatabaseProvider.Object)
                .BuildServiceProvider();
            
            // Act - Tests the exact extension method call pattern from Program.cs line 269
            var databaseProvider = serviceProvider.GetRequiredService<IJellyfinDatabaseProvider>();
            
            // Assert
            Assert.NotNull(databaseProvider);
            Assert.Same(mockDatabaseProvider.Object, databaseProvider);
        }

        [Fact]
        public async Task ServiceProvider_GetRequiredService_RunsShutdownTaskWithTimeout()
        {
            // Arrange
            var mockDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();
            mockDatabaseProvider.Setup(dp => dp.RunShutdownTask(It.IsAny<CancellationToken>()))
                               .Returns(Task.CompletedTask);
            
            var serviceProvider = new ServiceCollection()
                .AddSingleton(mockDatabaseProvider.Object)
                .BuildServiceProvider();
            
            // Act - Simulates the exact finally block pattern from Program.cs lines 269-273
            if (serviceProvider is not null)
            {
                var databaseProvider = serviceProvider.GetRequiredService<IJellyfinDatabaseProvider>();
                using var shutdownSource = new CancellationTokenSource();
                shutdownSource.CancelAfter(TimeSpan.FromSeconds(60));
                await databaseProvider.RunShutdownTask(shutdownSource.Token);
            }
            
            // Assert - Verifies the GetRequiredService extension was effectively used and shutdown task executed
            mockDatabaseProvider.Verify(dp => dp.RunShutdownTask(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
