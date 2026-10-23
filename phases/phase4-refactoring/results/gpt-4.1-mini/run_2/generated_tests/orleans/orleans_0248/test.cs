using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using Xunit;

namespace Orleans.Redis.Tests
{
    public class RedisGrainDirectoryProviderBuilderTests
    {
        [Fact]
        public void Configure_UsesGetConnectionStringFromIConfiguration_WhenConnectionNameProvidedAndConnectionStringEmpty()
        {
            // Arrange
            var builderMock = new Mock<ISiloBuilder>();
            Action<OptionsBuilder<RedisGrainDirectoryOptions>>? capturedConfigureAction = null;

            builderMock
                .Setup(b => b.AddRedisGrainDirectory(It.IsAny<string>(), It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()))
                .Callback<string, Action<OptionsBuilder<RedisGrainDirectoryOptions>>>((name, action) =>
                {
                    capturedConfigureAction = action;
                })
                .Returns(builderMock.Object);

            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.SetupGet(c => c["ServiceKey"]).Returns(string.Empty);
            configSectionMock.SetupGet(c => c["ConnectionName"]).Returns("MyConnectionName");
            configSectionMock.SetupGet(c => c["ConnectionString"]).Returns(string.Empty);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("MyConnectionName")).Returns("TestConnectionString");

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IConfiguration)))
                .Returns(configurationMock.Object);

            // Act
            var builder = (IProviderBuilder<ISiloBuilder>)Activator.CreateInstance(
                typeof(Orleans.Hosting.RedisGrainDirectoryProviderBuilder),
                nonPublic: true)!;
            builder.Configure(builderMock.Object, "TestName", configSectionMock.Object);

            Assert.NotNull(capturedConfigureAction);

            var services = new ServiceCollection();
            var optionsBuilder = new OptionsBuilder<RedisGrainDirectoryOptions>(services, null);
            capturedConfigureAction!(optionsBuilder);

            var options = new RedisGrainDirectoryOptions();

            // The Configure<IServiceProvider> call is internal, so we invoke the delegate by reflection
            var configureActionsField = typeof(OptionsBuilder<RedisGrainDirectoryOptions>)
                .GetField("_configureActions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(configureActionsField);
            var configureActions = configureActionsField.GetValue(optionsBuilder) as System.Collections.Generic.List<Delegate>;
            Assert.NotNull(configureActions);
            Assert.NotEmpty(configureActions);

            // The last configure action is the one we want to invoke
            var lastConfigure = configureActions[^1];
            lastConfigure.DynamicInvoke(options, serviceProviderMock.Object);

            // Assert
            Assert.NotNull(options.ConfigurationOptions);
            Assert.Equal("TestConnectionString", options.ConfigurationOptions.ToString());
        }
    }
}
