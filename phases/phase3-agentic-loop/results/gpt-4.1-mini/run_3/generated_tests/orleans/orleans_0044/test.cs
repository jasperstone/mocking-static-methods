using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Transactions.AzureStorage;
using Xunit;
using Moq;

namespace Orleans.Hosting
{
    public class AzureTableTransactionServicecollectionExtensionsTests
    {
        [Fact]
        public void AddAzureTableTransactionalStateStorage_InvokesGetRequiredServiceOnIServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            // Register IOptionsMonitor<AzureTableTransactionalStateOptions> with a mock that returns a default options instance
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableTransactionalStateOptions>>();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(new AzureTableTransactionalStateOptions());
            services.AddSingleton(optionsMonitorMock.Object);

            // Use reflection to get the internal method
            var method = typeof(AzureTableTransactionServicecollectionExtensions)
                .GetMethod("AddAzureTableTransactionalStateStorage", BindingFlags.Static | BindingFlags.NonPublic);

            Assert.NotNull(method);

            // Act
            var returnedServices = (IServiceCollection)method.Invoke(null, new object[] { services, "test", null });

            // Build provider to trigger service registrations
            var provider = returnedServices.BuildServiceProvider();

            // Resolve IConfigurationValidator to trigger the factory delegate that calls GetRequiredService
            var validator = provider.GetService<IConfigurationValidator>();

            // Assert
            Assert.NotNull(validator);
            optionsMonitorMock.Verify(m => m.Get("test"), Times.AtLeastOnce);
        }
    }
}
