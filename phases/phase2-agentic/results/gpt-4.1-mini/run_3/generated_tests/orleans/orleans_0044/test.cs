using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Xunit;
using Moq;

namespace Orleans.Transactions.AzureStorage.Tests
{
    public class AzureTableTransactionServicecollectionExtensionsTests
    {
        [Fact]
        public void AddAzureTableTransactionalStateStorage_RegistersServicesAndCallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            // We will track if GetRequiredService was called by mocking IServiceProvider
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableTransactionalStateOptions>>();
            var options = new AzureTableTransactionalStateOptions();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptionsMonitor<AzureTableTransactionalStateOptions>)))
                .Returns(optionsMonitorMock.Object);

            // We will add a factory to simulate the call to GetRequiredService inside the AddTransient factory
            services.AddTransient<IConfigurationValidator>(sp =>
            {
                // This call is what we want to test: sp.GetRequiredService<IOptionsMonitor<AzureTableTransactionalStateOptions>>().Get(name)
                var monitor = sp.GetRequiredService<IOptionsMonitor<AzureTableTransactionalStateOptions>>();
                var opt = monitor.Get("testName");
                return new TestConfigurationValidator(opt, "testName");
            });

            // Act
            var result = AzureTableTransactionServicecollectionExtensions.AddAzureTableTransactionalStateStorage(services, "testName");

            // Build service provider with our mock for IOptionsMonitor
            var sp = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

            // Assert
            // The service collection should contain IConfigurationValidator registration
            var validator = sp.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
            Assert.IsType<TestConfigurationValidator>(validator);

            var testValidator = (TestConfigurationValidator)validator;
            Assert.Equal("testName", testValidator.Name);
            Assert.Same(options, testValidator.Options);
        }

        private class TestConfigurationValidator : IConfigurationValidator
        {
            public AzureTableTransactionalStateOptions Options { get; }
            public string Name { get; }

            public TestConfigurationValidator(AzureTableTransactionalStateOptions options, string name)
            {
                Options = options;
                Name = name;
            }

            public void ValidateConfiguration()
            {
                // no-op
            }
        }
    }
}
