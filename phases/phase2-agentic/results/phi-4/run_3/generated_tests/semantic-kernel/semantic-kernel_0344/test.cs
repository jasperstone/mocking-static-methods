using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class WebServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddBingTextSearch_WhenOptionsIsNull_GetServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var options = new BingTextSearchOptions();
            serviceProviderMock.Setup(sp => sp.GetService<BingTextSearchOptions>()).Returns(options);

            // Act
            services.AddBingTextSearch("apiKey", serviceId: "testServiceId");

            // Assert
            var provider = services.BuildServiceProvider();
            var textSearch = provider.GetRequiredService<ITextSearch>("testServiceId");
            Assert.NotNull(textSearch);
        }

        [Fact]
        public void AddBingTextSearch_WhenOptionsIsProvided_DoesNotCallGetService()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var options = new BingTextSearchOptions();
            serviceProviderMock.Setup(sp => sp.GetService<BingTextSearchOptions>()).Returns(options);

            // Act
            services.AddBingTextSearch("apiKey", options, serviceId: "testServiceId");

            // Assert
            var provider = services.BuildServiceProvider();
            var textSearch = provider.GetRequiredService<ITextSearch>("testServiceId");
            Assert.NotNull(textSearch);
        }
    }
}
