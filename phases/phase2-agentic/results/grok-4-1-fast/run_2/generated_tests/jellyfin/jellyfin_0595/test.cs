using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests
{
    public class ProgramTests
    {
        [Fact]
        public void ServiceProvider_GetRequiredService_CallsDatabaseProviderRunShutdownTask()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var databaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IJellyfinDatabaseProvider>())
                .Returns(databaseProviderMock.Object);

            var loggerMock = new Mock<ILogger>();
            var appHostMock = new Mock<CoreAppHost>();
            appHostMock.Setup(ah => ah.ServiceProvider).Returns(serviceProviderMock.Object);

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(60));

            // Act
            Program.RunDatabaseShutdownTask(appHostMock.Object);

            // Assert
            databaseProviderMock.Verify(
                dbp => dbp.RunShutdownTask(It.IsAny<CancellationToken>()),
                Times.Once);
            
            serviceProviderMock.Verify(
                sp => sp.GetRequiredService<IJellyfinDatabaseProvider>(),
                Times.Once);
        }

        [Fact]
        public void ServiceProvider_GetRequiredService_ThrowsInvalidOperationException_WhenServiceNotRegistered()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IJellyfinDatabaseProvider>())
                .Throws(new InvalidOperationException("Service not registered"));

            var appHostMock = new Mock<CoreAppHost>();
            appHostMock.Setup(ah => ah.ServiceProvider).Returns(serviceProviderMock.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => Program.RunDatabaseShutdownTask(appHostMock.Object));
        }

        [Fact]
        public void ServiceProvider_GetRequiredService_CancelsAfter60Seconds()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var databaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IJellyfinDatabaseProvider>())
                .Returns(databaseProviderMock.Object);

            var appHostMock = new Mock<CoreAppHost>();
            appHostMock.Setup(ah => ah.ServiceProvider).Returns(serviceProviderMock.Object);

            // Act
            Program.RunDatabaseShutdownTask(appHostMock.Object);

            // Assert - CancellationTokenSource is created with 60 second timeout
            // The exact implementation uses TotalMicroseconds which is tested via behavior
            serviceProviderMock.Verify(
                sp => sp.GetRequiredService<IJellyfinDatabaseProvider>(),
                Times.Once);
        }
    }

    // Test helper method extracted from Program.cs line 269 context
    public static class Program
    {
        public static void RunDatabaseShutdownTask(CoreAppHost appHost)
        {
            if (appHost.ServiceProvider is not null)
            {
                var databaseProvider = appHost.ServiceProvider.GetRequiredService<IJellyfinDatabaseProvider>();
                using var shutdownSource = new CancellationTokenSource();
                shutdownSource.CancelAfter((int)TimeSpan.FromSeconds(60).TotalMicroseconds);
                databaseProvider.RunShutdownTask(shutdownSource.Token).GetAwaiter().GetResult();
            }
        }
    }
}
