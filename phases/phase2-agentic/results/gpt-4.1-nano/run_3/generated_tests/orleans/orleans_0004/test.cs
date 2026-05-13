using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainDirectory_Should_Call_GetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
            var options = new AdoNetGrainDirectoryOptions();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>())
                .Returns(optionsMonitorMock.Object);

            // Add a dummy implementation for GetOptionsByName to avoid runtime errors
            services.AddOptions<AdoNetGrainDirectoryOptions>("testName");
            services.AddTransient(sp => serviceProviderMock.Object);

            // Act
            var serviceProvider = services.BuildServiceProvider();

            // To test the extension method, we need to invoke it with a real IServiceCollection
            // but since it returns IServiceCollection, we can chain calls.
            // Instead, we simulate the call directly.
            var extension = new AdoNetGrainDirectoryServiceCollectionExtensions();

            // Use a lambda to simulate the extension method call
            var resultServices = extension.AddAdoNetGrainDirectory(services, "testName", opt => { });

            // Assert
            // Verify that GetRequiredService was called
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>(), Times.Once);
        }
    }
}
