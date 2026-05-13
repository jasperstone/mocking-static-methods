using Xunit;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Tests
{
    public class DefaultDisplayTemplatesTests
    {
        [Fact]
        public void ObjectTemplate_ShouldCallGetRequiredServiceForIViewBufferScope()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockViewEngine = new Mock<ICompositeViewEngine>();
            var mockViewBufferScope = new Mock<IViewBufferScope>();

            mockServiceProvider
                .Setup(x => x.GetRequiredService<ICompositeViewEngine>())
                .Returns(mockViewEngine.Object);

            mockServiceProvider
                .Setup(x => x.GetRequiredService<IViewBufferScope>())
                .Returns(mockViewBufferScope.Object);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(x => x.RequestServices).Returns(mockServiceProvider.Object);

            var mockViewContext = new ViewContext
            {
                HttpContext = mockHttpContext.Object
            };

            var mockHtmlHelper = new Mock<IHtmlHelper>();
            mockHtmlHelper.Setup(x => x.ViewContext).Returns(mockViewContext);

            var mockModelExplorer = new Mock<ModelExplorer>();
            mockModelExplorer.Setup(x => x.Model).Returns(new object());
            mockModelExplorer.Setup(x => x.Metadata).Returns(new ModelMetadata(new EmptyModelMetadataProvider(), new ModelAttributes(), typeof(object), null, null, null));

            var mockViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                ModelExplorer = mockModelExplorer.Object,
                TemplateInfo = new TemplateInfo()
            };

            mockHtmlHelper.Setup(x => x.ViewData).Returns(mockViewData);

            // Act
            var result = DefaultDisplayTemplates.ObjectTemplate(mockHtmlHelper.Object);

            // Assert
            mockServiceProvider.Verify(x => x.GetRequiredService<IViewBufferScope>(), Times.Once);
        }
    }
}
