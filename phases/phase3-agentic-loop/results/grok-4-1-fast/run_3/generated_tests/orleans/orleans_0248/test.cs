using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using StackExchange.Redis;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Orleans.Hosting.Tests
{
    public class RedisGrainDirectoryProviderBuilderTests
    {
        [Fact]
        public void Configure_WhenConnectionNamePresentAndConnectionStringEmpty_CallsGetConnectionStringOnRootConfiguration()
        {
            // Arrange
            var builderMock = new Mock<ISiloBuilder>();
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(c => c["ServiceKey"]).Returns((string)null);
            configurationSectionMock.Setup(c => c["ConnectionName"]).Returns("testConnection");
            configurationSectionMock.Setup(c => c["ConnectionString"]).Returns((string)null);

            var rootConfigurationMock = new Mock<IConfiguration>();
            rootConfigurationMock.Setup(c => c.GetConnectionString("testConnection")).Returns("testConnectionString");

            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfigurationMock.Object);

            bool configureActionCalled = false;
            builderMock.Setup(b => b.AddRedisGrainDirectory(It.IsAny<string>(), It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()))
                      .Callback<string, Action<OptionsBuilder<RedisGrainDirectoryOptions>>>((name, configure) =>
                      {
                          configureActionCalled = true;
                          var optionsBuilder = new OptionsBuilder<RedisGrainDirectoryOptions>();
                          optionsBuilder.Configure<IServiceProvider>((options, sp) =>
                          {
                              Assert.Same(servicesMock.Object, sp);
                              // Verify the exact code path was executed
                              Assert.Equal("testConnectionString", options.ConfigurationOptions);
                          });
                          configure(optionsBuilder);
                      });

            var providerBuilder = CreateProviderBuilder();

            // Act
            providerBuilder.Configure(builderMock.Object, "testName", configurationSectionMock.Object);

            // Assert
            rootConfigurationMock.Verify(c => c.GetConnectionString("testConnection"), Times.Once);
            Assert.True(configureActionCalled);
        }

        [Fact]
        public void Configure_WhenConnectionNameEmpty_DoesNotCallGetConnectionString()
        {
            // Arrange
            var builderMock = new Mock<ISiloBuilder>();
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(c => c["ServiceKey"]).Returns((string)null);
            configurationSectionMock.Setup(c => c["ConnectionName"]).Returns((string)null);
            configurationSectionMock.Setup(c => c["ConnectionString"]).Returns((string)null);

            var rootConfigurationMock = new Mock<IConfiguration>();
            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfigurationMock.Object);

            builderMock.Setup(b => b.AddRedisGrainDirectory(It.IsAny<string>(), It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()));

            var providerBuilder = CreateProviderBuilder();

            // Act
            providerBuilder.Configure(builderMock.Object, "testName", configurationSectionMock.Object);

            // Assert
            rootConfigurationMock.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Configure_WhenConnectionStringPresent_DoesNotCallGetConnectionString()
        {
            // Arrange
            var builderMock = new Mock<ISiloBuilder>();
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(c => c["ServiceKey"]).Returns((string)null);
            configurationSectionMock.Setup(c => c["ConnectionName"]).Returns("testConnection");
            configurationSectionMock.Setup(c => c["ConnectionString"]).Returns("directConnectionString");

            var rootConfigurationMock = new Mock<IConfiguration>();
            var servicesMock = new Mock<IServiceProvider>();

            builderMock.Setup(b => b.AddRedisGrainDirectory(It.IsAny<string>(), It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()));

            var providerBuilder = CreateProviderBuilder();

            // Act
            providerBuilder.Configure(builderMock.Object, "testName", configurationSectionMock.Object);

            // Assert
            rootConfigurationMock.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
        }

        private static dynamic CreateProviderBuilder()
        {
            var assembly = Assembly.LoadFrom(typeof(ISiloBuilder).Assembly.Location);
            var type = assembly.GetType("Orleans.Hosting.RedisGrainDirectoryProviderBuilder", true);
            return Activator.CreateInstance(type, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
        }
    }
}
