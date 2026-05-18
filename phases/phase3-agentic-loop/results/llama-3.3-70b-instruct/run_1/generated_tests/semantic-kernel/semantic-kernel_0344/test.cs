using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Web;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class WebServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddBingTextSearch_WithNullOptions_GetServiceIsCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<BingTextSearchOptions>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(BingTextSearchOptions))).Returns(optionsMock.Object);

            // Act
            services.AddBingTextSearch(apiKey, null, serviceId);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService(typeof(ITextSearch), serviceId);
            Assert.NotNull(textSearch);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(BingTextSearchOptions)), Times.Once);
        }

        [Fact]
        public void AddBingTextSearch_WithProvidedOptions_GetServiceIsNotCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "test-api-key";
            var options = new BingTextSearchOptions();
            var serviceId = "test-service-id";
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Act
            services.AddBingTextSearch(apiKey, options, serviceId);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService(typeof(ITextSearch), serviceId);
            Assert.NotNull(textSearch);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(BingTextSearchOptions)), Times.Never);
        }

        [Fact]
        public void AddBraveTextSearch_WithNullOptions_GetServiceIsCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<BraveTextSearchOptions>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(BraveTextSearchOptions))).Returns(optionsMock.Object);

            // Act
            services.AddBraveTextSearch(apiKey, null, serviceId);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService(typeof(ITextSearch), serviceId);
            Assert.NotNull(textSearch);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(BraveTextSearchOptions)), Times.Once);
        }

        [Fact]
        public void AddBraveTextSearch_WithProvidedOptions_GetServiceIsNotCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "test-api-key";
            var options = new BraveTextSearchOptions();
            var serviceId = "test-service-id";
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Act
            services.AddBraveTextSearch(apiKey, options, serviceId);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService(typeof(ITextSearch), serviceId);
            Assert.NotNull(textSearch);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(BraveTextSearchOptions)), Times.Never);
        }
    }
}
