using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    public class DefaultDisplayTemplatesTests
    {
        [Fact]
        public void ObjectTemplate_CallsGetRequiredServiceOnServiceProvider()
        {
            // Arrange
            var mockViewEngine = new Mock<ICompositeViewEngine>();
            var mockViewBufferScope = new Mock<IViewBufferScope>();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ICompositeViewEngine)))
                .Returns(mockViewEngine.Object);
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IViewBufferScope)))
                .Returns(mockViewBufferScope.Object);

            // Setup GetRequiredService extension method behavior by mocking IServiceProvider.GetService
            // Because GetRequiredService calls GetService internally and throws if null
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ICompositeViewEngine>())
                .Returns(mockViewEngine.Object);
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IViewBufferScope>())
                .Returns(mockViewBufferScope.Object);

            var httpContextMock = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
            httpContextMock.SetupGet(c => c.RequestServices).Returns(serviceProviderMock.Object);

            var viewContext = new ViewContext
            {
                HttpContext = httpContextMock.Object
            };

            var modelMetadataMock = new Mock<ModelMetadata>(ModelMetadataIdentity.ForType(typeof(object)));
            modelMetadataMock.SetupGet(m => m.NullDisplayText).Returns("NullDisplayText");
            modelMetadataMock.SetupGet(m => m.HtmlEncode).Returns(false);
            modelMetadataMock.SetupGet(m => m.Properties).Returns(new ModelPropertyCollection(new List<ModelExplorer>()));

            var modelExplorer = new ModelExplorer(
                new EmptyModelMetadataProvider(),
                container: null,
                metadata: modelMetadataMock.Object,
                model: new object());

            var templateInfo = new TemplateInfo
            {
                TemplateDepth = 1
            };

            var viewDataDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                ModelExplorer = modelExplorer,
                TemplateInfo = templateInfo
            };

            var htmlHelperMock = new Mock<IHtmlHelper>();
            htmlHelperMock.SetupGet(h => h.ViewData).Returns(viewDataDictionary);
            htmlHelperMock.SetupGet(h => h.ViewContext).Returns(viewContext);
            htmlHelperMock.Setup(h => h.Encode(It.IsAny<string>())).Returns<string>(s => s);

            // Act
            var result = DefaultDisplayTemplates.ObjectTemplate(htmlHelperMock.Object);

            // Assert
            Assert.NotNull(result);
            // Verify that GetRequiredService was called for ICompositeViewEngine and IViewBufferScope
            serviceProviderMock.Verify(sp => sp.GetRequiredService<ICompositeViewEngine>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IViewBufferScope>(), Times.Once);
        }
    }
}
