using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class AzureTableTransactionServicecollectionExtensionsTests
    {
        [Fact]
        public void AddAzureTableTransactionalStateStorage_RegistersValidatorUsingGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableTransactionalStateOptions>>();
            optionsMonitorMock.Setup(m => m.Get("testName")).Returns(new AzureTableTransactionalStateOptions());
            services.AddSingleton<IOptionsMonitor<AzureTableTransactionalStateOptions>>(optionsMonitorMock.Object);

            // Act
            var result = services.AddAzureTableTransactionalStateStorage("testName");

            // Assert - Build provider and resolve to trigger the factory that calls GetRequiredService
            Assert.Same(services, result);
            var serviceProvider = services.BuildServiceProvider();
            var validator = serviceProvider.GetRequiredService<Orleans.Runtime.IConfigurationValidator>();
            Assert.NotNull(validator);
            optionsMonitorMock.Verify(m => m.Get("testName"), Times.Once);
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_ThrowsWhenIOptionsMonitorMissing()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = services.AddAzureTableTransactionalStateStorage("testName");
            
            // Assert
            var serviceProvider = services.BuildServiceProvider();
            Assert.ThrowsAny<Exception>(() => serviceProvider.GetRequiredService<Orleans.Runtime.IConfigurationValidator>());
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_CallsConfigureOptions_WhenProvided()
        {
            // Arrange
            var services = new ServiceCollection();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableTransactionalStateOptions>>();
            services.AddSingleton<IOptionsMonitor<AzureTableTransactionalStateOptions>>(optionsMonitorMock.Object);
            bool configureCalled = false;

            void Configure(OptionsBuilder<AzureTableTransactionalStateOptions> builder)
            {
                configureCalled = true;
                builder.Configure(opt => opt.TableName = "CustomTable");
            }

            // Act
            var result = services.AddAzureTableTransactionalStateStorage("testName", Configure);

            // Assert
            Assert.True(configureCalled);
            Assert.Same(services, result);
        }
    }
}
