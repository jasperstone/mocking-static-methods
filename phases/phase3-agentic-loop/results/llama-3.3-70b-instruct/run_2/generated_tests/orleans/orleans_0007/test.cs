using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Persistence.AdoNet;
using Moq;

namespace Orleans.Tests
{
    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainStorage_ValidOptions_AddsGrainStorage()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "TestStorage";
            var configureOptions = new Action<Microsoft.Extensions.Options.OptionsBuilder<Orleans.Configuration.AdoNetGrainStorageOptions>>(ob => ob.Configure(options => { }));

            // Act
            services.AddAdoNetGrainStorage(name, configureOptions);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var grainStorageFactory = serviceProvider.GetService<Orleans.Runtime.IGrainStorageFactory>();
            var grainStorage = grainStorageFactory.CreateGrainStorage(name);
            Assert.NotNull(grainStorage);
        }

        [Fact]
        public void AddAdoNetGrainStorage_InvalidOptions_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "TestStorage";
            var configureOptions = new Action<Microsoft.Extensions.Options.OptionsBuilder<Orleans.Configuration.AdoNetGrainStorageOptions>>(ob => ob.Configure(options => { throw new Exception("Invalid options"); }));

            // Act and Assert
            Assert.Throws<Exception>(() => services.AddAdoNetGrainStorage(name, configureOptions));
        }

        [Fact]
        public void AddAdoNetGrainStorage_GetRequiredService_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "TestStorage";
            var configureOptions = new Action<Microsoft.Extensions.Options.OptionsBuilder<Orleans.Configuration.AdoNetGrainStorageOptions>>(ob => ob.Configure(options => { }));
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<Microsoft.Extensions.Options.IOptionsMonitor<Orleans.Configuration.AdoNetGrainStorageOptions>>();
            optionsMonitorMock.Setup(om => om.Get(name)).Returns(new Orleans.Configuration.AdoNetGrainStorageOptions());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<Orleans.Configuration.AdoNetGrainStorageOptions>>()).Returns(optionsMonitorMock.Object);

            // Act
            services.AddAdoNetGrainStorage(name, configureOptions);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<Orleans.Configuration.AdoNetGrainStorageOptions>>(), Times.Once);
        }
    }
}
