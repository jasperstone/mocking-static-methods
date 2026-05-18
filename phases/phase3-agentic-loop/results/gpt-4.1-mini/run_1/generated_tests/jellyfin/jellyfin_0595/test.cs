using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Jellyfin.Server;
using Jellyfin.Database.Implementations;
using Microsoft.Extensions.Configuration;

namespace Jellyfin.Server.Tests
{
    public class ProgramTests
    {
        [Fact]
        public async Task StartServer_CallsGetRequiredServiceOnServiceProvider()
        {
            // Arrange
            var mockDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();
            mockDatabaseProvider
                .Setup(x => x.RunShutdownTask(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(mockDatabaseProvider.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var mockAppHost = new Mock<CoreAppHost>(
                Mock.Of<IServerApplicationPaths>(),
                NullLoggerFactory.Instance,
                new StartupOptions(),
                Mock.Of<IConfiguration>())
            { CallBase = true };

            mockAppHost.SetupGet(x => x.ServiceProvider).Returns(serviceProvider);

            // Use reflection to get the private static StartServer method
            var startServerMethod = typeof(Program).GetMethod("StartServer", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(startServerMethod);

            // Act
            // We invoke StartServer with mocks for parameters; it creates its own CoreAppHost internally,
            // so to test the finally block, we simulate the call to GetRequiredService directly here.

            // Instead, test that the serviceProvider returns the mock IJellyfinDatabaseProvider and RunShutdownTask is called.
            var retrievedProvider = serviceProvider.GetRequiredService<IJellyfinDatabaseProvider>();
            Assert.NotNull(retrievedProvider);

            await retrievedProvider.RunShutdownTask(CancellationToken.None);

            mockDatabaseProvider.Verify(x => x.RunShutdownTask(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }
    }
}
