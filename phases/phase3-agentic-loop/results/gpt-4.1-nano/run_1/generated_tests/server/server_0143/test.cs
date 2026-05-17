using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Bit.Core;
using Bit.Core.Entities;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.Auth.Services;
using Microsoft.Extensions.Logging;

namespace Bit.SharedWeb.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDatabaseRepositories_ShouldConfigureProvider()
        {
            var services = new ServiceCollection();
            var globalSettings = new Mock<IGlobalSettings>();
            globalSettings.Setup(g => g.SelfHosted).Returns(false);
            var result = services.AddDatabaseRepositories(globalSettings.Object);
            Assert.NotNull(result);
        }

        [Fact]
        public void AddBaseServices_ShouldRegisterServices()
        {
            var services = new ServiceCollection();
            var globalSettings = new Mock<IGlobalSettings>();
            globalSettings.Setup(g => g).Returns(new GlobalSettings());
            services.AddBaseServices(globalSettings.Object);
            var provider = services.BuildServiceProvider();

            var cipherService = provider.GetService<ICipherService>();
            Assert.NotNull(cipherService);
            var groupService = provider.GetService<IGroupService>();
            Assert.NotNull(groupService);
            var eventService = provider.GetService<IEventService>();
            Assert.NotNull(eventService);
        }

        [Fact]
        public void AddTokenizers_ShouldRegisterTokenFactories()
        {
            var services = new ServiceCollection();
            services.AddTokenizers();
            var provider = services.BuildServiceProvider();

            var factory = provider.GetService<IDataProtectorTokenFactory<OrgDeleteTokenable>>();
            Assert.NotNull(factory);
        }

        [Fact]
        public void GetRequiredService_ShouldReturnService()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            var provider = services.BuildServiceProvider();

            var logger = provider.GetRequiredService<ILogger<ServiceCollectionExtensionsTests>>();
            Assert.NotNull(logger);
        }
    }
}
