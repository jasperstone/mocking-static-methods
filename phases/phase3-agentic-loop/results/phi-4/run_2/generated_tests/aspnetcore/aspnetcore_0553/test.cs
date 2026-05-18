using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Tests
{
    public class DefaultDisplayTemplatesTests
    {
        [Fact]
        public void BuildDisplayTemplate_ShouldRetrieveCorrectServices()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var expectedViewEngine = new Mock<ICompositeViewEngine>();
            var expectedViewBufferScope = new Mock<IViewBufferScope>();
            var expectedMetadataProvider = new Mock<IModelMetadataProvider>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ICompositeViewEngine>())
                .Returns(expectedViewEngine.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IViewBufferScope>())
                .Returns(expectedViewBufferScope.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IModelMetadataProvider>())
                .Returns(expectedMetadataProvider.Object);

            var viewContextMock = new Mock<ViewContext>();
            viewContextMock.Setup(vc => vc.HttpContext.RequestServices).Returns(serviceProviderMock.Object);

            var viewDataMock = new Mock<ViewDataDictionary>();
            var modelMetadataProvider = new Mock<IModelMetadataProvider>();
            var model = new object(); // Replace with a suitable model type if needed

            var modelExplorerMock = new Mock<ModelExplorer>(
                modelMetadataProvider.Object,
                container: null,
                metadata: modelMetadataProvider.Object.GetMetadataForType(model.GetType()),
                model: model);

            // Act
            var result = DefaultDisplayTemplates.BuildDisplayTemplate(
                serviceProviderMock.Object.GetRequiredService<ICompositeViewEngine>(),
                serviceProviderMock.Object.GetRequiredService<IViewBufferScope>(),
                viewContextMock.Object,
                viewDataMock.Object,
                modelExplorerMock.Object,
                "testField",
                null,
                true,
                null);

            // Assert
            Assert.NotNull(result);
        }
    }
}
