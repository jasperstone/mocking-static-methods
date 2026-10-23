using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class WebServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddBingTextSearch_WithNullOptions_GetServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "apiKey";
            var serviceId = "serviceId";
            var options = new BingTextSearchOptions();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(BingTextSearchOptions))).Returns(options);

            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            services.AddBingTextSearch(apiKey, null, serviceId);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(BingTextSearchOptions)), Times.Once);
        }

        [Fact]
        public void AddBingTextSearch_WithNonNullOptions_GetServiceNotCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "apiKey";
            var serviceId = "serviceId";
            var options = new BingTextSearchOptions();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(BingTextSearchOptions))).Returns(options);

            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            services.AddBingTextSearch(apiKey, options, serviceId);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(BingTextSearchOptions)), Times.Never);
        }
    }
}
