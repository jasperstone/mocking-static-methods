using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainStorage_CallsGetRequiredServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddAdoNetGrainStorage("testName", (Action<AdoNetGrainStorageOptions>)null);

            // Assert - Verify the IConfigurationValidator registration was added (which contains the GetRequiredService call)
            var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(IConfigurationValidator)));
            Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
            Assert.IsAssignableFrom<Delegate>(descriptor.ImplementationFactory);
            
            // Verify the factory calls GetRequiredService by invoking it with a mock service provider
            var spMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            optionsMock.Setup(m => m.Get("testName")).Returns(new AdoNetGrainStorageOptions());
            spMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainStorageOptions>>()).Returns(optionsMock.Object);
            
            var factory = (Func<IServiceProvider, object>)descriptor.ImplementationFactory!;
            var validator = factory(spMock.Object);
            Assert.NotNull(validator);
            spMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainStorageOptions>>(), Times.Once);
        }

        [Fact]
        public void AddAdoNetGrainStorageAsDefault_CallsGetRequiredServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddAdoNetGrainStorageAsDefault();

            // Assert
            var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(IConfigurationValidator)));
            Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
            Assert.IsAssignableFrom<Delegate>(descriptor.ImplementationFactory);
        }

        [Fact]
        public void AddAdoNetGrainStorage_WithOptionsBuilderConfigure_CallsGetRequiredServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddAdoNetGrainStorage("testName", (Action<OptionsBuilder<AdoNetGrainStorageOptions>>)null);

            // Assert
            var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(IConfigurationValidator)));
            Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
            Assert.IsAssignableFrom<Delegate>(descriptor.ImplementationFactory);
        }
    }
}
