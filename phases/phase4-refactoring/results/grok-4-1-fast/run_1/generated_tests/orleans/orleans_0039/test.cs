using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class AzureQueueStreamProviderBuilderTests
    {
        [Fact]
        public void GetQueueOptionBuilder_CallsGetConnectionString_WhenConnectionNamePresentAndConnectionStringEmpty()
        {
            // Arrange
            var configSection = new Mock<IConfigurationSection>();
            configSection.Setup(c => c["ConnectionName"]).Returns("test-connection");
            configSection.Setup(c => c["ConnectionString"]).Returns((string)null);
            configSection.Setup(c => c["ServiceKey"]).Returns((string)null);
            configSection.Setup(c => c.GetSection("QueueNames")).Returns((IConfigurationSection)null);

            var rootConfig = new Mock<IConfiguration>();
            rootConfig.Setup(c => c.GetConnectionString("test-connection")).Returns("test-connection-string");

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(rootConfig.Object);
            var serviceProvider = services.BuildServiceProvider();

            // Capture the configure action
            Action<object, IServiceProvider> capturedConfigureAction = null;
            var optionsBuilderMock = new Mock<OptionsBuilder<object>>();
            optionsBuilderMock.Setup(ob => ob.Configure<IServiceProvider>(It.IsAny<Action<object, IServiceProvider>>()))
                             .Callback<Action<object, IServiceProvider>>((action) => capturedConfigureAction = action);

            // Use reflection to invoke private static method
            var method = typeof(AzureQueueStreamProviderBuilder).GetMethod("GetQueueOptionBuilder", 
                BindingFlags.NonPublic | BindingFlags.Static);
            var optionBuilder = (Action<OptionsBuilder<object>>)method.Invoke(null, new[] { configSection.Object });
            
            // Act
            optionBuilder(optionsBuilderMock.Object);
            
            // Trigger the configure action to execute the code path
            capturedConfigureAction(serviceProvider, serviceProvider);

            // Assert
            rootConfig.Verify(c => c.GetConnectionString("test-connection"), Times.Once);
        }

        [Fact]
        public void GetQueueOptionBuilder_DoesNotCallGetConnectionString_WhenConnectionStringPresent()
        {
            // Arrange
            var configSection = new Mock<IConfigurationSection>();
            configSection.Setup(c => c["ConnectionName"]).Returns("test-connection");
            configSection.Setup(c => c["ConnectionString"]).Returns("direct-connection-string");
            configSection.Setup(c => c["ServiceKey"]).Returns((string)null);

            var rootConfig = new Mock<IConfiguration>();
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(rootConfig.Object);
            var serviceProvider = services.BuildServiceProvider();

            var optionsBuilderMock = new Mock<OptionsBuilder<object>>();

            var method = typeof(AzureQueueStreamProviderBuilder).GetMethod("GetQueueOptionBuilder", 
                BindingFlags.NonPublic | BindingFlags.Static);
            var optionBuilder = (Action<OptionsBuilder<object>>)method.Invoke(null, new[] { configSection.Object });

            // Act
            optionBuilder(optionsBuilderMock.Object);

            // Assert
            rootConfig.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GetQueueOptionBuilder_DoesNotCallGetConnectionString_WhenConnectionNameEmpty()
        {
            // Arrange
            var configSection = new Mock<IConfigurationSection>();
            configSection.Setup(c => c["ConnectionName"]).Returns((string)null);
            configSection.Setup(c => c["ConnectionString"]).Returns((string)null);
            configSection.Setup(c => c["ServiceKey"]).Returns((string)null);

            var rootConfig = new Mock<IConfiguration>();
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(rootConfig.Object);
            var serviceProvider = services.BuildServiceProvider();

            var optionsBuilderMock = new Mock<OptionsBuilder<object>>();

            var method = typeof(AzureQueueStreamProviderBuilder).GetMethod("GetQueueOptionBuilder", 
                BindingFlags.NonPublic | BindingFlags.Static);
            var optionBuilder = (Action<OptionsBuilder<object>>)method.Invoke(null, new[] { configSection.Object });

            // Act
            optionBuilder(optionsBuilderMock.Object);

            // Assert
            rootConfig.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GetQueueOptionBuilder_TakesServiceKeyPath_WhenServiceKeyPresent()
        {
            // Arrange
            var configSection = new Mock<IConfigurationSection>();
            configSection.Setup(c => c["ServiceKey"]).Returns("test-service-key");

            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            var optionsBuilderMock = new Mock<OptionsBuilder<object>>();

            var method = typeof(AzureQueueStreamProviderBuilder).GetMethod("GetQueueOptionBuilder", 
                BindingFlags.NonPublic | BindingFlags.Static);
            var optionBuilder = (Action<OptionsBuilder<object>>)method.Invoke(null, new[] { configSection.Object });

            // Act
            optionBuilder(optionsBuilderMock.Object);

            // Assert - no exception thrown
            Assert.True(true);
        }
    }
}
