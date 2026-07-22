using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Moq;
using Microsoft.Extensions.Options;

namespace Orleans.Tests
{
    public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainDirectory_ValidInput_ServiceProviderCreated()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "TestName";
            var configureOptions = new Action<OptionsBuilder<AdoNetGrainDirectoryOptions>>(options => { });

            // Act
            services.AddAdoNetGrainDirectory(name, configureOptions);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var adoNetGrainDirectoryOptionsValidator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(adoNetGrainDirectoryOptionsValidator);
        }

        [Fact]
        public void AddAdoNetGrainDirectory_GetRequiredServiceCalled_ServiceProviderCreated()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "TestName";
            var configureOptions = new Action<OptionsBuilder<AdoNetGrainDirectoryOptions>>(options => { });
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>()).Returns(new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>().Object);

            // Act
            services.AddAdoNetGrainDirectory(name, configureOptions);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var adoNetGrainDirectoryOptionsValidator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(adoNetGrainDirectoryOptionsValidator);
        }
    }
}
