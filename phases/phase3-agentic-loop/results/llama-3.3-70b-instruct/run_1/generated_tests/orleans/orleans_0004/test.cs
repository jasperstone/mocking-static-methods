using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Xunit;

namespace Orleans.Tests
{
    public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainDirectory_ValidOptions_AddsGrainDirectory()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "TestGrainDirectory";
            var configureOptions = new Action<Microsoft.Extensions.Options.OptionsBuilder<AdoNetGrainDirectoryOptions>>(options =>
            {
                options.Configure(name, options =>
                {
                    // Configure options
                });
            });

            // Act
            services.AddAdoNetGrainDirectory(name, configureOptions);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var grainDirectory = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(grainDirectory);
        }

        [Fact]
        public void AddAdoNetGrainDirectory_InvalidOptions_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "TestGrainDirectory";
            var configureOptions = new Action<Microsoft.Extensions.Options.OptionsBuilder<AdoNetGrainDirectoryOptions>>(options =>
            {
                // Do not configure options
            });

            // Act and Assert
            Assert.Throws<Exception>(() => services.AddAdoNetGrainDirectory(name, configureOptions));
        }
    }
}
