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

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDatabaseRepositories_ShouldConfigureProvider()
        {
            var services = new ServiceCollection();
            var globalSettings = new Mock<IGlobalSettings>().Object;
            var provider = services.AddDatabaseRepositories(globalSettings);
            var serviceProvider = services.BuildServiceProvider();

            Assert.NotNull(provider);
            Assert.IsType<SupportedDatabaseProviders>(provider);
        }

        [Fact]
        public void AddBaseServices_ShouldRegisterCoreServices()
        {
            var services = new ServiceCollection();
            var globalSettings = new Mock<IGlobalSettings>().Object;

            services.AddBaseServices(globalSettings);
            var provider = services.BuildServiceProvider();

            Assert.NotNull(provider.GetService<ICipherService>());
            Assert.NotNull(provider.GetService<IUserService>());
            Assert.NotNull(provider.GetService<IReportingService>());
        }

        [Fact]
        public void AddTokenizers_ShouldRegisterTokenFactories()
        {
            var services = new ServiceCollection();

            services.AddTokenizers();
            var provider = services.BuildServiceProvider();

            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<OrgDeleteTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<SsoTokenable>>());
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
