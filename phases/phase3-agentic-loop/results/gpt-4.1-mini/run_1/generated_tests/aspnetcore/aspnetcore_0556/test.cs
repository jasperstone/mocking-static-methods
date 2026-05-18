using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    public class DefaultDisplayTemplatesTests
    {
        [Fact]
        public void ObjectTemplate_CallsGetRequiredServiceOnRequestServices()
        {
            // Arrange
            var mockViewEngine = new Mock<ICompositeViewEngine>(MockBehavior.Strict);
            var mockViewBufferScope = new Mock<IViewBufferScope>(MockBehavior.Strict);

            var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ICompositeViewEngine)))
                .Returns(mockViewEngine.Object);
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IViewBufferScope)))
                .Returns(mockViewBufferScope.Object);

            var httpContextMock = new Mock<HttpContext>(MockBehavior.Strict);
            httpContextMock.SetupGet(c => c.RequestServices).Returns(serviceProviderMock.Object);

            var viewContext = new ViewContext
            {
                HttpContext = httpContextMock.Object
            };

            var modelMetadataProvider = new EmptyModelMetadataProvider();
            var modelMetadata = modelMetadataProvider.GetMetadataForType(typeof(object));
            var modelExplorer = new ModelExplorer(
                modelMetadataProvider,
                container: null,
                metadata: modelMetadata,
                model: new object());

            var templateInfo = new TemplateInfo
            {
                TemplateDepth = 0
            };

            var viewDataDictionary = new ViewDataDictionary(modelMetadataProvider, new ModelStateDictionary())
            {
                TemplateInfo = templateInfo,
                ModelExplorer = modelExplorer,
                ModelMetadata = modelMetadata
            };

            var htmlHelperMock = new Mock<IHtmlHelper>(MockBehavior.Strict);
            htmlHelperMock.SetupGet(h => h.ViewData).Returns(viewDataDictionary);
            htmlHelperMock.SetupGet(h => h.ViewContext).Returns(viewContext);
            htmlHelperMock.Setup(h => h.Encode(It.IsAny<string>())).Returns<string>(s => s);

            // Act
            var result = DefaultDisplayTemplates.ObjectTemplate(htmlHelperMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IHtmlContent>(result);

            // Verify that GetService was called for ICompositeViewEngine and IViewBufferScope
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ICompositeViewEngine)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IViewBufferScope)), Times.AtLeastOnce);
        }
    }
}
