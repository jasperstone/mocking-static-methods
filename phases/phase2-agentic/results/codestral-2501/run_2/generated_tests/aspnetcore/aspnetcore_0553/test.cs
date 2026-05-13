using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Collections;
using System;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Tests
{
    public class DefaultDisplayTemplatesTests
    {
        [Fact]
        public void CollectionTemplate_ShouldCallGetRequiredServiceForICompositeViewEngine()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockViewEngine = new Mock<ICompositeViewEngine>();
            var mockViewBufferScope = new Mock<IViewBufferScope>();
            var mockModelMetadataProvider = new Mock<IModelMetadataProvider>();

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<ICompositeViewEngine>())
                .Returns(mockViewEngine.Object);

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<IViewBufferScope>())
                .Returns(mockViewBufferScope.Object);

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<IModelMetadataProvider>())
                .Returns(mockModelMetadataProvider.Object);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(hc => hc.RequestServices).Returns(mockServiceProvider.Object);

            var mockViewContext = new Mock<ViewContext>();
            mockViewContext.Setup(vc => vc.HttpContext).Returns(mockHttpContext.Object);

            var mockHtmlHelper = new Mock<IHtmlHelper>();
            mockHtmlHelper.Setup(hh => hh.ViewContext).Returns(mockViewContext.Object);

            var mockModelMetadata = new Mock<ModelMetadata>();
            mockModelMetadata.Setup(mm => mm.ElementMetadata).Returns(mockModelMetadata.Object);
            mockModelMetadata.Setup(mm => mm.IsNullableValueType).Returns(false);
            mockModelMetadata.Setup(mm => mm.ModelType).Returns(typeof(object));

            var mockViewData = new Mock<ViewDataDictionary>();
            mockViewData.Setup(vd => vd.ModelMetadata).Returns(mockModelMetadata.Object);
            mockViewData.Setup(vd => vd.Model).Returns(new List<string> { "item1", "item2" });

            mockHtmlHelper.Setup(hh => hh.ViewData).Returns(mockViewData.Object);

            // Act
            var result = DefaultDisplayTemplates.CollectionTemplate(mockHtmlHelper.Object);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<ICompositeViewEngine>(), Times.Once);
        }
    }
}
