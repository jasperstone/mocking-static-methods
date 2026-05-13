using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Plugins.Web.Bing;

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
            serviceProviderMock
                .Setup(sp => sp.GetService<BingTextSearchOptions>())
                .Returns(new BingTextSearchOptions());

            services.AddSingleton(serviceProviderMock.Object);

            var apiKey = "test-api-key";

            // Act
            services.AddBingTextSearch(apiKey);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService<BingTextSearchOptions>(), Times.Once);
        }

        [Fact]
        public void AddBingTextSearch_WhenOptionsIsProvided_GetServiceNotCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var options = new BingTextSearchOptions();

            services.AddSingleton(serviceProviderMock.Object);

            var apiKey = "test-api-key";

            // Act
            services.AddBingTextSearch(apiKey, options);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService<BingTextSearchOptions>(), Times.Never);
        }
    }
}
