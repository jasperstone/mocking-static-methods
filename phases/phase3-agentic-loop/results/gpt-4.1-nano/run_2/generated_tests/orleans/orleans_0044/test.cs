using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.Transactions.AzureStorage;
using Orleans.Transactions.Abstractions;
using System;

namespace Orleans.Tests
{
    public class AzureTableTransactionServicecollectionExtensionsTests
    {
        [Fact]
        public void AddAzureTableTransactionalStateStorage_Should_Call_GetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var called = false;

            // Setup a mock IServiceProvider
            services.AddTransient<IConfigurationValidator>(sp =>
            {
                // Inside the lambda, simulate the call to GetRequiredService
                var spMock = new ServiceCollection()
                    .AddOptions<AzureTableTransactionalStateOptions>("test")
                    .BuildServiceProvider();

                // We need to simulate the sp.GetRequiredService<IOptionsMonitor<AzureTableTransactionalStateOptions>>()
                var optionsMonitor = spMock.GetRequiredService<IOptionsMonitor<AzureTableTransactionalStateOptions>>();

                // Call the method under test, which calls sp.GetRequiredService
                var extension = new AzureTableTransactionServicecollectionExtensions();

                // Use a custom IServiceProvider that tracks the call
                var mockProvider = new MockServiceProvider(() =>
                {
                    called = true;
                    return spMock;
                });

                // Call the lambda
                var validator = new AzureTableTransactionalStateOptionsValidator(
                    optionsMonitor.Get("test"), "test");
                return validator;
            });

            // Act
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.True(called, "GetRequiredService was not called");
        }

        // Helper class to mock IServiceProvider
        private class MockServiceProvider : IServiceProvider
        {
            private readonly Func<IServiceProvider> _getServiceFunc;

            public MockServiceProvider(Func<IServiceProvider> getServiceFunc)
            {
                _getServiceFunc = getServiceFunc;
            }

            public object GetService(Type serviceType)
            {
                return _getServiceFunc();
            }
        }
    }
}
