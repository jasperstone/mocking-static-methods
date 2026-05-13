using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using Xunit;

namespace Orleans.GrainDirectory.AdoNet.Tests
{
    public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
    {
        private class DummyOptions : AdoNetGrainDirectoryOptions { }

        [Fact]
        public void AddAdoNetGrainDirectory_InvokesConfigureOptions_AndAddsServices()
        {
            // Arrange
            var servicesMock = new Mock<IServiceCollection>(MockBehavior.Strict);
            var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>(MockBehavior.Strict);

            var optionsName = "testName";

            // Setup AddOptions call to return a dummy OptionsBuilder
            var optionsBuilder = new OptionsBuilder<AdoNetGrainDirectoryOptions>(optionsName, servicesMock.Object);
            var configureOptionsCalled = false;

            // Setup AddOptions extension method on IServiceCollection
            servicesMock.Setup(s => s.AddOptions<AdoNetGrainDirectoryOptions>(optionsName))
                .Returns(optionsBuilder)
                .Callback(() => configureOptionsCalled = true);

            // Setup AddTransient call on IServiceCollection
            servicesMock.Setup(s => s.AddTransient(
                It.IsAny<Func<IServiceProvider, IConfigurationValidator>>()))
                .Returns(servicesMock.Object);

            // Setup ConfigureNamedOptionForLogging call on IServiceCollection
            servicesMock.Setup(s => s.ConfigureNamedOptionForLogging<AdoNetGrainDirectoryOptions>(optionsName))
                .Returns(servicesMock.Object);

            // Setup AddGrainDirectory call on IServiceCollection
            servicesMock.Setup(s => s.AddGrainDirectory(
                optionsName,
                It.IsAny<Func<IServiceProvider, string, object>>()))
                .Returns(servicesMock.Object);

            // Setup GetRequiredService on IServiceProvider to return the mocked options monitor
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>())
                .Returns(optionsMonitorMock.Object);

            // Setup Get on options monitor to return dummy options
            optionsMonitorMock.Setup(m => m.Get(optionsName))
                .Returns(new AdoNetGrainDirectoryOptions());

            // Act
            var result = AdoNetGrainDirectoryServiceCollectionExtensions.AddAdoNetGrainDirectory(
                servicesMock.Object,
                optionsName,
                ob => { /* no-op configure */ });

            // Assert
            Assert.Same(servicesMock.Object, result);
            Assert.True(configureOptionsCalled);

            // Verify AddTransient was called with a factory that calls GetRequiredService
            servicesMock.Verify(s => s.AddTransient(It.IsAny<Func<IServiceProvider, IConfigurationValidator>>()), Times.Once);

            // Verify ConfigureNamedOptionForLogging was called
            servicesMock.Verify(s => s.ConfigureNamedOptionForLogging<AdoNetGrainDirectoryOptions>(optionsName), Times.Once);

            // Verify AddGrainDirectory was called
            servicesMock.Verify(s => s.AddGrainDirectory(optionsName, It.IsAny<Func<IServiceProvider, string, object>>()), Times.Once);
        }
    }
}
