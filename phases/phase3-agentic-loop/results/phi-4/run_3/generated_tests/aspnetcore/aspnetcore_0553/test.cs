using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Tests
{
    public class DefaultDisplayTemplatesTests
    {
        [Fact]
        public void CollectionTemplate_ShouldRetrieveICompositeViewEngineAndIViewBufferScope()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockViewEngine = new Mock<ICompositeViewEngine>();
            var mockViewBufferScope = new Mock<IViewBufferScope>();
            var mockMetadataProvider = new Mock<IModelMetadataProvider>();

            mockServiceProvider
                .Setup(s => s.GetRequiredService(typeof(ICompositeViewEngine)))
                .Returns(mockViewEngine.Object);

            mockServiceProvider
                .Setup(s => s.GetRequiredService(typeof(IViewBufferScope)))
                .Returns(mockViewBufferScope.Object);

            mockServiceProvider
                .Setup(s => s.GetRequiredService(typeof(IModelMetadataProvider)))
                .Returns(mockMetadataProvider.Object);

            var mockHttpContext = new Mock<HttpContext>();
            var mockRequestServices = new Mock<IServiceProvider>();
            mockHttpContext.SetupGet(h => h.RequestServices).Returns(mockRequestServices.Object);

            var mockViewContext = new Mock<ViewContext>();
            mockViewContext.SetupGet(v => v.HttpContext).Returns(mockHttpContext.Object);

            var mockViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());

            var model = new List<string> { "item1", "item2" };

            var htmlHelper = new HtmlHelper(mockViewContext.Object, mockViewData);

            // Act
            var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelper);

            // Assert
            mockServiceProvider.Verify(s => s.GetRequiredService(typeof(ICompositeViewEngine)), Times.Once);
            mockServiceProvider.Verify(s => s.GetRequiredService(typeof(IViewBufferScope)), Times.Once);
        }
    }
}
