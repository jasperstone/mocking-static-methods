using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Microsoft.SemanticKernel.Plugins.Web;
using Moq;

namespace Microsoft.SemanticKernel.Tests
{
    public class WebServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddBingTextSearch_ShouldRegisterBingTextSearch()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var apiKey = "test-api-key";
            var options = new BingTextSearchOptions();

            // Act
            serviceCollection.AddBingTextSearch(apiKey, options);
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(BingTextSearchOptions))).Returns(options);
            var serviceProvider = serviceProviderMock.Object;
            var textSearch = serviceProvider.GetService<ITextSearch>();

            // Assert
            Assert.NotNull(textSearch);
            Assert.IsType<BingTextSearch>(textSearch);
        }

        [Fact]
        public void AddBingTextSearch_ShouldUseServiceProviderOptions_WhenOptionsNotProvided()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var apiKey = "test-api-key";
            var options = new BingTextSearchOptions();
            serviceCollection.AddSingleton(options);

            // Act
            serviceCollection.AddBingTextSearch(apiKey);
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(BingTextSearchOptions))).Returns(options);
            var serviceProvider = serviceProviderMock.Object;
            var textSearch = serviceProvider.GetService<ITextSearch>();

            // Assert
            Assert.NotNull(textSearch);
            Assert.IsType<BingTextSearch>(textSearch);
        }

        [Fact]
        public void AddBraveTextSearch_ShouldRegisterBraveTextSearch()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var apiKey = "test-api-key";
            var options = new BraveTextSearchOptions();

            // Act
            serviceCollection.AddBraveTextSearch(apiKey, options);
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(BraveTextSearchOptions))).Returns(options);
            var serviceProvider = serviceProviderMock.Object;
            var textSearch = serviceProvider.GetService<ITextSearch>();

            // Assert
            Assert.NotNull(textSearch);
            Assert.IsType<BraveTextSearch>(textSearch);
        }

        [Fact]
        public void AddBraveTextSearch_ShouldUseServiceProviderOptions_WhenOptionsNotProvided()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var apiKey = "test-api-key";
            var options = new BraveTextSearchOptions();
            serviceCollection.AddSingleton(options);

            // Act
            serviceCollection.AddBraveTextSearch(apiKey);
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(BraveTextSearchOptions))).Returns(options);
            var serviceProvider = serviceProviderMock.Object;
            var textSearch = serviceProvider.GetService<ITextSearch>();

            // Assert
            Assert.NotNull(textSearch);
            Assert.IsType<BraveTextSearch>(textSearch);
        }
    }
}
