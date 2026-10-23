using System;
using Jellyfin.Server;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using MediaBrowser.Controller;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Server.Tests
{
    public class CoreAppHostTests
    {
        [Fact]
        public void GetRequiredService_ShouldReturnIJellyfinDatabaseProvider()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var databaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IJellyfinDatabaseProvider))).Returns(databaseProviderMock.Object);

            var appHost = new CoreAppHost(
                Mock.Of<IServerApplicationPaths>(),
                Mock.Of<ILoggerFactory>(),
                Mock.Of<IStartupOptions>(),
                Mock.Of<IConfiguration>());

            // Act
            var result = appHost.ServiceProvider.GetRequiredService<IJellyfinDatabaseProvider>();

            // Assert
            Assert.Same(databaseProviderMock.Object, result);
        }
    }
}
