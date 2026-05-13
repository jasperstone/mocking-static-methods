using System;
using System.Collections;
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
        public void CollectionTemplate_ThrowsInvalidOperationException_IfModelIsNotEnumerable()
        {
            // Arrange
            var htmlHelper = new Mock<IHtmlHelper>();
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = new object()
            };
            var modelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(object));
            viewData.ModelMetadata = modelMetadata;
            htmlHelper.Setup(h => h.ViewData).Returns(viewData);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                DefaultDisplayTemplates.CollectionTemplate(htmlHelper.Object));
            Assert.Contains("TypeMustImplementIEnumerable", ex.Message);
        }

        [Fact]
        public void CollectionTemplate_CallsGetRequiredServiceOnServiceProvider()
        {
            // Arrange
            var model = new List<string> { "item1", "item2" };
            var modelMetadataProviderMock = new Mock<IModelMetadataProvider>();
            var elementMetadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(string));
            modelMetadataProviderMock.Setup(m => m.GetMetadataForType(typeof(string))).Returns(elementMetadata);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IModelMetadataProvider))).Returns(modelMetadataProviderMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ICompositeViewEngine))).Returns(Mock.Of<ICompositeViewEngine>());
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IViewBufferScope))).Returns(Mock.Of<IViewBufferScope>());

            // Setup extension method GetRequiredService to call GetService and cast
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IModelMetadataProvider>()).Returns(modelMetadataProviderMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ICompositeViewEngine>()).Returns(Mock.Of<ICompositeViewEngine>());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IViewBufferScope>()).Returns(Mock.Of<IViewBufferScope>());

            var httpContextMock = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
            httpContextMock.SetupGet(c => c.RequestServices).Returns(serviceProviderMock.Object);

            var viewContext = new ViewContext()
            {
                HttpContext = httpContextMock.Object
            };

            var templateInfo = new TemplateInfo();
            var modelExplorer = new ModelExplorer(new EmptyModelMetadataProvider(), null, elementMetadata, model);
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model,
                ModelMetadata = elementMetadata,
                ModelExplorer = modelExplorer,
                TemplateInfo = templateInfo
            };

            var htmlHelperMock = new Mock<IHtmlHelper>();
            htmlHelperMock.Setup(h => h.ViewData).Returns(viewData);
            htmlHelperMock.Setup(h => h.ViewContext).Returns(viewContext);

            // Act
            var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelperMock.Object);

            // Assert
            Assert.NotNull(result);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IModelMetadataProvider>(), Times.AtLeastOnce());
            serviceProviderMock.Verify(sp => sp.GetRequiredService<ICompositeViewEngine>(), Times.AtLeastOnce());
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IViewBufferScope>(), Times.AtLeastOnce());
        }
    }
}
