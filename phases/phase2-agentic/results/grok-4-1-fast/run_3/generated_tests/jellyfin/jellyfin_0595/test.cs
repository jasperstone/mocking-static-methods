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
        public void ServiceProvider_GetRequiredService_IJellyfinDatabaseProvider_Success()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var databaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IJellyfinDatabaseProvider)))
                .Returns(databaseProviderMock.Object);

            var loggerMock = new Mock<ILogger<CoreAppHost>>();

            // Create CoreAppHost instance (minimal constructor simulation)
            var appPaths = new Mock<IServerApplicationPaths>().Object;
            var loggerFactory = new Mock<ILoggerFactory>().Object;
            var options = new StartupOptions();
            var startupConfig = new Mock<IConfiguration>().Object;
            
            var appHost = new CoreAppHost(appPaths, loggerFactory, options, startupConfig);
            
            // Use reflection to set ServiceProvider since it's internal/private
            typeof(CoreAppHost).GetProperty("ServiceProvider")?
                .SetValue(appHost, serviceProviderMock.Object);

            // Act
            var databaseProvider = appHost.ServiceProvider!.GetRequiredService<IJellyfinDatabaseProvider>();

            // Assert
            Assert.NotNull(databaseProvider);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IJellyfinDatabaseProvider)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IJellyfinDatabaseProvider>(), Times.Once);
        }

        [Fact]
        public void ServiceProvider_GetRequiredService_IJellyfinDatabaseProvider_ThrowsWhenServiceMissing()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IJellyfinDatabaseProvider)))
                .Returns((IJellyfinDatabaseProvider)null);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService(typeof(IJellyfinDatabaseProvider)))
                .Throws(new InvalidOperationException("Service not registered."));

            var loggerMock = new Mock<ILogger<CoreAppHost>>();

            var appPaths = new Mock<IServerApplicationPaths>().Object;
            var loggerFactory = new Mock<ILoggerFactory>().Object;
            var options = new StartupOptions();
            var startupConfig = new Mock<IConfiguration>().Object;
            
            var appHost = new CoreAppHost(appPaths, loggerFactory, options, startupConfig);
            
            typeof(CoreAppHost).GetProperty("ServiceProvider")?
                .SetValue(appHost, serviceProviderMock.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => 
                appHost.ServiceProvider!.GetRequiredService<IJellyfinDatabaseProvider>());
        }

        [Fact]
        public async Task RunShutdownTask_CalledWithCancellationToken()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var databaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            databaseProviderMock
                .Setup(dp => dp.RunShutdownTask(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IJellyfinDatabaseProvider>())
                .Returns(databaseProviderMock.Object);

            var appPaths = new Mock<IServerApplicationPaths>().Object;
            var loggerFactory = new Mock<ILoggerFactory>().Object;
            var options = new StartupOptions();
            var startupConfig = new Mock<IConfiguration>().Object;
            
            var appHost = new CoreAppHost(appPaths, loggerFactory, options, startupConfig);
            typeof(CoreAppHost).GetProperty("ServiceProvider")?
                .SetValue(appHost, serviceProviderMock.Object);

            // Act (simulate the finally block logic)
            using var shutdownSource = new CancellationTokenSource();
            shutdownSource.CancelAfter(TimeSpan.FromSeconds(60));
            var databaseProvider = appHost.ServiceProvider!.GetRequiredService<IJellyfinDatabaseProvider>();
            await databaseProvider.RunShutdownTask(shutdownSource.Token);

            // Assert
            databaseProviderMock.Verify(dp => dp.RunShutdownTask(It.Is<CancellationToken>(ct => 
                ct.CanBeCanceled)), Times.Once);
        }
    }
}
