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
            var webHostBuilderMock = new Mock<IWebHostBuilder>();
            var webHostBuilderDefaultsMock = new Mock<IWebHostBuilder>();
            var appHostMock = new Mock<CoreAppHost>(new ServerApplicationPaths(), new LoggerFactory(), new StartupOptions(), null);

            // Setup the host builder to return a mock host
            hostMock.Setup(h => h.Services).Returns(serviceProviderMock.Object);
            hostMock.Setup(h => h.Dispose()).Verifiable();

            // Setup the static Host.CreateDefaultBuilder to return our mock
            var hostBuilder = new Mock<IHostBuilder>();
            hostBuilder.Setup(hb => hb.UseConsoleLifetime()).Returns(hostBuilder.Object);
            hostBuilder.Setup(hb => hb.ConfigureServices(It.IsAny<Action<IServiceCollection>>()))
                .Callback<Action<IServiceCollection>>(action =>
                {
                    var services = new ServiceCollection();
                    action(services);
                })
                .Returns(hostBuilder.Object);
            hostBuilder.Setup(hb => hb.ConfigureWebHostDefaults(It.IsAny<Action<IWebHostBuilder>>()))
                .Callback<Action<IWebHostBuilder>>(configure =>
                {
                    var webHostBuilder = new Mock<IWebHostBuilder>();
                    webHostBuilder.Setup(w => w.ConfigureWebHostBuilder(It.IsAny<CoreAppHost>(), It.IsAny<IConfiguration>(), It.IsAny<IServerApplicationPaths>(), It.IsAny<ILogger>()))
                        .Callback<CoreAppHost, IConfiguration, IServerApplicationPaths, ILogger>((app, config, paths, logger) =>
                        {
                            // Simulate calling GetRequiredService
                            var services = new ServiceCollection();
                            services.AddSingleton<IJellyfinDatabaseProvider>(databaseProviderMock.Object);
                            var provider = services.BuildServiceProvider();
                            var dbProvider = provider.GetRequiredService<IJellyfinDatabaseProvider>();
                            Assert.NotNull(dbProvider);
                        });
                })
                .Returns(hostBuilder.Object);

            // Setup Host.CreateDefaultBuilder to return our mock
            var hostBuilderFactory = new Mock<Func<IHostBuilder>>();
            hostBuilderFactory.Setup(f => f()).Returns(hostBuilder.Object);

            // Act
            await Program.StartServer(new ServerApplicationPaths(), new StartupOptions(), null);

            // Assert
            databaseProviderMock.Verify(dp => dp.RunShutdownTask(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
