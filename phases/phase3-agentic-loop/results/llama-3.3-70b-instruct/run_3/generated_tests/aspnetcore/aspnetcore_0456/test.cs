using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Reflection;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Filters
{
    public class MiddlewareFilterConfigurationProviderTests
    {
        [Fact]
        public void CreateConfigureDelegate_WithValidConfigureMethod_ReturnsConfigureDelegate()
        {
            // Arrange
            var configurationType = typeof(ValidConfigureClass);
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var applicationBuilder = new ApplicationBuilder(serviceProvider);

            // Act
            var configureDelegate = MiddlewareFilterConfigurationProvider.CreateConfigureDelegate(configurationType);

            // Assert
            Assert.NotNull(configureDelegate);
            configureDelegate(applicationBuilder);
        }

        [Fact]
        public void CreateConfigureDelegate_WithInvalidConfigureMethod_ThrowsInvalidOperationException()
        {
            // Arrange
            var configurationType = typeof(InvalidConfigureClass);

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => MiddlewareFilterConfigurationProvider.CreateConfigureDelegate(configurationType));
        }

        [Fact]
        public void Invoke_WithValidParameters_ResolvesServicesCorrectly()
        {
            // Arrange
            var configurationType = typeof(ValidConfigureClass);
            var serviceProvider = new ServiceCollection().AddScoped<IService, Service>().BuildServiceProvider();
            var applicationBuilder = new ApplicationBuilder(serviceProvider);
            var instance = Activator.CreateInstance(configurationType);
            var configureDelegateBuilder = MiddlewareFilterConfigurationProvider.GetConfigureDelegateBuilder(configurationType);

            // Act
            configureDelegateBuilder.Build(instance)(applicationBuilder);

            // Assert
            // We can't directly verify the resolved services, but we can verify that the configure method was invoked correctly
            Assert.True(((ValidConfigureClass)instance).ConfigureInvoked);
        }

        [Fact]
        public void Invoke_WithInvalidParameters_ThrowsInvalidOperationException()
        {
            // Arrange
            var configurationType = typeof(InvalidConfigureClass);
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var applicationBuilder = new ApplicationBuilder(serviceProvider);
            var instance = Activator.CreateInstance(configurationType);
            var configureDelegateBuilder = MiddlewareFilterConfigurationProvider.GetConfigureDelegateBuilder(configurationType);

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => configureDelegateBuilder.Build(instance)(applicationBuilder));
        }

        private class ValidConfigureClass
        {
            public bool ConfigureInvoked { get; private set; }

            public void Configure(IApplicationBuilder applicationBuilder, IService service)
            {
                ConfigureInvoked = true;
            }
        }

        private class InvalidConfigureClass
        {
            public void Configure(IApplicationBuilder applicationBuilder, object invalidService)
            {
            }
        }

        private interface IService
        {
        }

        private class Service : IService
        {
        }
    }
}
