using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Jellyfin.Server;
using Jellyfin.Server.Extensions;

namespace Jellyfin.Tests
{
    public class ProgramTests
    {
        [Fact]
        public async Task StartServer_CallsGetRequiredService()
        {
            // Arrange
            var servicesMock = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var databaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var hostBuilderMock = new Mock<IHostBuilder>();
            var hostMock = new Mock<IHost>();
            var appHostMock = new Mock<CoreAppHost>();
            var loggerMock = new Mock<ILogger>();

            // Setup service provider to return the mock database provider
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IJellyfinDatabaseProvider>())
                .Returns(databaseProviderMock.Object);

            // Setup host creation to return a mock host
            hostMock.Setup(h => h.Services).Returns(serviceProviderMock.Object);
            hostMock.Setup(h => h.Dispose());

            // Setup Host.CreateDefaultBuilder to return a mock host builder
            var hostBuilder = new Mock<IHostBuilder>();
            hostBuilder.Setup(hb => hb.UseConsoleLifetime()).Returns(hostBuilder.Object);
            hostBuilder.Setup(hb => hb.ConfigureServices(It.IsAny<Action<IServiceCollection>>()))
                .Callback<Action<IServiceCollection>>(services => services(servicesMock));
            hostBuilder.Setup(hb => hb.Build()).Returns(hostMock.Object);

            // Replace Host.CreateDefaultBuilder to return our mock
            var createDefaultBuilderCalled = false;
            Func<IHostBuilder> createDefaultBuilder = () =>
            {
                createDefaultBuilderCalled = true;
                return hostBuilder.Object;
            };

            // Act
            // Call StartServer with a custom host creation function
            await Program.StartServerForTestAsync(
                appPaths: null,
                options: new StartupOptions(),
                startupConfig: null,
                createHostFunc: createDefaultBuilder,
                logger: loggerMock.Object,
                appHostFactory: () => new CoreAppHost(null, null, null, null));

            // Assert
            Assert.True(createDefaultBuilderCalled);
            // Verify that GetRequiredService was called
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IJellyfinDatabaseProvider>(), Times.Once);
        }
    }

    // Extension method to facilitate testing StartServer
    public static class ProgramExtensions
    {
        public static async Task StartServerForTestAsync(
            this Program program,
            object appPaths,
            StartupOptions options,
            IConfiguration startupConfig,
            Func<IHostBuilder> createHostFunc,
            ILogger logger,
            Func<CoreAppHost> appHostFactory)
        {
            await program.StartServerInternalAsync(appPaths, options, startupConfig, createHostFunc, logger, appHostFactory);
        }

        private static async Task StartServerInternalAsync(
            this Program program,
            object appPaths,
            StartupOptions options,
            IConfiguration startupConfig,
            Func<IHostBuilder> createHostFunc,
            ILogger logger,
            Func<CoreAppHost> appHostFactory)
        {
            // The actual implementation of StartServer, adapted for testing
            using var appHost = appHostFactory();

            var hostBuilder = createHostFunc();

            var host = hostBuilder.Build();

            var services = host.Services;

            // Call the method under test
            await Program.StartServerAsync(host, appHost, services, logger);
        }
    }
}
