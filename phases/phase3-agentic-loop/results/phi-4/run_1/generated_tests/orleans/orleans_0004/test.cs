using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainDirectory_AddsConfigurationValidatorAndGrainDirectory()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "TestGrainDirectory";
            var optionsBuilderMock = new Mock<Action<OptionsBuilder<AdoNetGrainDirectoryOptions>>>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>())
                .Returns(optionsMonitorMock.Object);

            optionsMonitorMock
                .Setup(m => m.Get(name))
                .Returns(new AdoNetGrainDirectoryOptions());

            // Act
            services.AddAdoNetGrainDirectory(name, optionsBuilderMock.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var validator = serviceProvider.GetRequiredService<IConfigurationValidator>();

            Assert.NotNull(validator);
            optionsMonitorMock.Verify(m => m.Get(name), Times.Once);
        }
    }
}
