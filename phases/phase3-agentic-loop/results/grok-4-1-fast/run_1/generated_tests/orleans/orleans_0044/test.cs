using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using Orleans.Runtime;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class AzureTableTransactionServicecollectionExtensionsTests
    {
        [Fact]
        public void AddAzureTableTransactionalStateStorage_RegistersValidatorWithGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockOptionsMonitor = new Mock<IOptionsMonitor<object>>();
            mockOptionsMonitor.Setup(m => m.Get(It.IsAny<string>())).Returns(new object());
            services.AddSingleton<IOptionsMonitor<object>>(mockOptionsMonitor.Object);

            string name = "test";

            // Act
            var result = services.AddAzureTableTransactionalStateStorage(name);

            // Assert
            Assert.Same(services, result);

            // Build provider and resolve to trigger the factory lambda on line 22
            // This calls sp.GetRequiredService<IOptionsMonitor<AzureTableTransactionalStateOptions>>()
            using var sp = services.BuildServiceProvider();
            var validator = sp.GetRequiredService<IConfigurationValidator>();
            Assert.NotNull(validator);

            mockOptionsMonitor.Verify(m => m.Get(name), Times.Once);
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_WithConfigureOptions_CallsDelegate()
        {
            // Arrange
            var services = new ServiceCollection();
            bool called = false;
            Action<object> configure = _ => called = true;

            // Act
            services.AddAzureTableTransactionalStateStorage("test", configure);

            // Assert
            Assert.True(called);
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_WithoutConfigureOptions_Succeeds()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = services.AddAzureTableTransactionalStateStorage("test");

            // Assert
            Assert.Same(services, result);
            using var sp = services.BuildServiceProvider();
            Assert.Throws<InvalidOperationException>(() => sp.GetRequiredService<IConfigurationValidator>());
        }
    }
}
